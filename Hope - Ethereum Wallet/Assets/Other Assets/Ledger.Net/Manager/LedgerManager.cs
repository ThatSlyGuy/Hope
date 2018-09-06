﻿using HidLibrary;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public sealed class LedgerManager
    {
        private readonly SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);

        public IHidDevice LedgerHidDevice { get; }
        public ICoinUtility CoinUtility { get; }
        public ICoinInfo CurrentCoin { get; private set; }

        public LedgerManager(IHidDevice ledgerHidDevice) : this(ledgerHidDevice, null)
        {
        }

        public LedgerManager(IHidDevice ledgerHidDevice, ICoinUtility coinUtility)
        {
            LedgerHidDevice = ledgerHidDevice;
            CoinUtility = coinUtility ?? new DefaultCoinUtility();

            SetCoinNumber(60);
        }

        private async Task WriteRequestAsync<TWrite>(TWrite message) where TWrite : RequestBase
        {
            var packetIndex = 0;
            byte[] data = null;
            using (var memoryStream = new MemoryStream(message.ToAPDU()))
            {
                do
                {
                    data = Helpers.GetRequestDataPacket(memoryStream, packetIndex);
                    packetIndex++;
                    await HID_Write(data).ConfigureAwait(false);
                } while (memoryStream.Position != memoryStream.Length);
            }
        }

        private async Task<byte[]> ReadResponseAsync()
        {
            var remaining = 0;
            var packetIndex = 0;
            using (var response = new MemoryStream())
            {
                do
                {
                    var packet = await HID_Read().ConfigureAwait(false);

                    if (packet?.Length == 0)
                        throw new Exception("Invalid response data packet!");

                    var responseDataPacket = Helpers.GetResponseDataPacket(packet, packetIndex, ref remaining);
                    packetIndex++;

                    if (responseDataPacket == null)
                    {
                        return null;
                    }

                    response.Write(responseDataPacket, 0, responseDataPacket.Length);

                } while (remaining != 0);

                return response.ToArray();
            }
        }

        private async Task<TResponse> SendRequestAsync<TResponse>(RequestBase request) where TResponse : ResponseBase
        {
            await _SemaphoreSlim.WaitAsync();

            try
            {
                await WriteRequestAsync(request);
                var responseData = await ReadResponseAsync();
                return (TResponse)Activator.CreateInstance(typeof(TResponse), responseData);
            }
            finally
            {
                _SemaphoreSlim.Release();
            }
        }

        public void SetCoinNumber(uint coinNumber)
        {
            CurrentCoin = CoinUtility.GetCoinInfo(coinNumber);
        }

        /// <summary>
        /// This will set the coin based on the currently open app. Note: this only currently works with Bitcoin based Ledger apps.
        /// </summary>
        public async Task SetCoinNumber()
        {
            var getCoinVersionRequest = await SendRequestAsync<GetCoinVersionResponse, GetCoinVersionRequest>(new GetCoinVersionRequest());
            CurrentCoin = CoinUtility.GetCoinInfo(getCoinVersionRequest.ShortCoinName);
        }

        public Task<string> GetAddressAsync(uint account, uint index)
        {
            return GetAddressAsync(account, false, index, false);
        }

        public async Task<string> GetAddressAsync(uint account, bool isChange, uint index, bool showDisplay)
        {
            byte[] data = Helpers.GetDerivationPathData(CurrentCoin.App, CurrentCoin.CoinNumber, account, index, isChange, CurrentCoin.IsSegwit);

            GetPublicKeyResponseBase response;
            if (CurrentCoin.App == App.Ethereum)
                response = await SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(showDisplay, false, data));
            else
                response = await SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(showDisplay, BitcoinAddressType.Segwit, data));

            return response.IsSuccess ? response.Address : null;
        }

        public Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
           where TResponse : ResponseBase
           where TRequest : RequestBase
        {
            return SendRequestAsync<TResponse>(request);
        }

        private async Task<byte[]> HID_Read()
        {
            var result = await LedgerHidDevice.ReadAsync().ConfigureAwait(false);

            if (result.Status == HidDeviceData.ReadStatus.Success)
            {
                if (result.Data?.Length - 1 <= 0)
                    return new byte[0];

                return result.Data.Skip(1).ToArray();
            }

            return new byte[0];
        }

        private async Task<int> HID_Write(byte[] buffer)
        {
            var sent = new byte[1].Concat(buffer).ToArray();

            if (!await LedgerHidDevice.WriteAsync(sent).ConfigureAwait(false))
                return -1;

            return sent.Length;
        }
    }
}
