﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Trezor.Net
{
    public class DefaultCoinUtility : ICoinUtility
    {
        public Dictionary<uint, CoinInfo> Coins { get; } = new Dictionary<uint, CoinInfo>();

        public DefaultCoinUtility()
        {
            var assembly = typeof(TrezorManager).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("Trezor.Net.Resources.Coins.xml");
            string xml;
            using (var reader = new StreamReader(stream))
            {
                xml = reader.ReadToEnd();
            }
            var coinsList = DeserialiseObject<List<CoinInfo>>(xml);

            foreach (var coinInfo in coinsList)
            {
                Coins.Add(coinInfo.CoinType, new CoinInfo(coinInfo.CoinName, AddressType.Bitcoin, coinInfo.IsSegwit, coinInfo.CoinType));
            }

            Coins.Add(60, new CoinInfo("Ethereum", AddressType.Ethereum, false, 60));
            Coins.Add(61, new CoinInfo("Ethereum Classic", AddressType.Ethereum, false, 61));
        }

        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            CoinInfo coinInfo;
            if (Coins.TryGetValue(coinNumber, out coinInfo))
            {
                return coinInfo;
            }

            throw new NotImplementedException($"The coin number {coinNumber} has not been filled in for {nameof(DefaultCoinUtility)}. Please create a class that implements ICoinUtility, or call an overload that specifies coin information.");
        }

        public static SerialiseType DeserialiseObject<SerialiseType>(string objectXml)
        {
            var serializer = new XmlSerializer(typeof(SerialiseType));
            var sr = new StringReader(objectXml);
            var retVal = serializer.Deserialize(sr);
            return (SerialiseType)retVal;
        }
    }
}
