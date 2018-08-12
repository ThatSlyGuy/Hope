﻿using Hope.Utils.Random.Abstract;
using Org.BouncyCastle.Crypto.Digests;

namespace Hope.Utils.Random
{
    /// <summary>
    /// Utility class for generating random byte data.
    /// </summary>
    public static class RandomBytes
    {
        /// <summary>
        /// Class which generates random bytes using the Blake2 hash function (https://en.wikipedia.org/wiki/BLAKE_(hash_function)).
        /// <para> This is the 10th fastest class for generating random byte data, being ~6.9x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered a very secure hash algorithm. It is arguably the most secure hash algorithm used for password hashing.
        /// </summary>
        public sealed class Blake2 : RandomBytesBase<Blake2bDigest> { }

        /// <summary>
        /// Class which generates random bytes using the MD5 hash function (https://en.wikipedia.org/wiki/MD5).
        /// <para> This is the 3rd fastest class for generating random byte data, being ~3.95x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is not considered a very secure algorithm, while still being much more secure than the <see cref="Fast"/> algorithm.
        /// </summary>
        public sealed class MD5 : RandomBytesBase<MD5Digest> { }

        /// <summary>
        /// Class which generates random bytes using the RIPEMD256 hash function (https://en.wikipedia.org/wiki/RIPEMD).
        /// <para> This is the 9th fastest class for generating random byte data, being ~6.45x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered not as secure as the SHA family of hash functions, while it is still often used for PGP encryption.
        /// </summary>
        public sealed class RIPEMD256 : RandomBytesBase<RipeMD256Digest> { }

        /// <summary>
        /// Class which generates random bytes using the RIPEMD320 hash function (https://en.wikipedia.org/wiki/RIPEMD).
        /// <para> This is the 11th fastest class for generating random byte data, being ~8.07x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered not as secure as the SHA family of hash functions, while it is still often used for PGP encryption.
        /// </summary>
        public sealed class RIPEMD320 : RandomBytesBase<RipeMD320Digest> { }

        /// <summary>
        /// Class which generates random bytes using the SHA1 hash function (https://en.wikipedia.org/wiki/SHA-1).
        /// <para> This is the 4th fastest class for generating random byte data, being ~4.63x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is not considered a very secure algorithm, for more security consider using <see cref="SHA256"/>/<see cref="SHA512"/> or <see cref="SHA3"/> variants instead.
        /// </summary>
        public sealed class SHA1 : RandomBytesBase<Sha1Digest> { }

        /// <summary>
        /// Class which generates random bytes using the SHA3 hash function (https://en.wikipedia.org/wiki/SHA-3).
        /// <para> This is the 5th fastest class for generating random byte data, being ~4.67x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered a very secure hash algorithm as it is a subset of the Keccak family of hashing functions.
        /// </summary>
        public sealed class SHA3 : RandomBytesBase<Sha3Digest> { }

        /// <summary>
        /// Class which generates random bytes using the SHA256 hash function (https://en.wikipedia.org/wiki/SHA-2).
        /// <para> This is the 6th fastest class for generating random byte data, being ~4.95x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered a moderately secure hash algorithm. While it has yet to be broken, <see cref="SHA3"/> offers higher level of security.
        /// </summary>
        public sealed class SHA256 : RandomBytesBase<Sha256Digest> { }

        /// <summary>
        /// Class which generates random bytes using the SHA512 hash function (https://en.wikipedia.org/wiki/SHA-2).
        /// <para> This is the 6th fastest class for generating random byte data, being ~5x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered a moderately secure hash algorithm. While it has yet to be broken, <see cref="SHA3"/> offers higher level of security.
        /// </summary>
        public sealed class SHA512 : RandomBytesBase<Sha512Digest> { }

        /// <summary>
        /// Class which generates random bytes using the SHAKE hash function (https://en.wikipedia.org/wiki/SHA-3).
        /// <para> This is the 8th fastest class for generating random byte data, being ~6.32x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is considered a very secure hash algorithm as it is a subset of the Keccak family of hashing functions.
        /// </summary>
        public sealed class Shake : RandomBytesBase<ShakeDigest> { }

        /// <summary>
        /// Class which generates random bytes using the chinese SM3 hash function (https://tools.ietf.org/id/draft-oscca-cfrg-sm3-01.html).
        /// <para> This is the 7th fastest class for generating random byte data, being ~6x slower than the <see cref="Fast"/> algorithm. </para>
        /// This appears to be a secure algorithm, while it is not widely adopted and tested compared to other hash functions.
        /// </summary>
        public sealed class SM3 : RandomBytesBase<SM3Digest> { }

        /// <summary>
        /// Class which generates random bytes using the Tiger hash function (https://en.wikipedia.org/wiki/Tiger_(hash_function)).
        /// <para> This is the 2nd fastest class for generating random byte data, being ~3.6x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is generally considered less secure than <see cref="RIPEMD256"/> and <see cref="RIPEMD320"/> variants.
        /// </summary>
        public sealed class Tiger : RandomBytesBase<TigerDigest> { }

        /// <summary>
        /// Class which generates random bytes using the Whirlpool hash function (https://en.wikipedia.org/wiki/Whirlpool_(hash_function)).
        /// <para> This is the slowest class for generating random byte data, being ~18.2x slower than the <see cref="Fast"/> algorithm. </para>
        /// This is a very niche algorithm, and has not seen wide use while being fairly secure.
        /// </summary>
        public sealed class Whirlpool : RandomBytesBase<WhirlpoolDigest> { }

        /// <summary>
        /// Class which generates random bytes using an insecure, yet very fast algorithm. 
        /// Should only be used if the random byte data does not need to be secure.
        /// </summary>
        public static class Fast
        {
            /// <summary>
            /// Generates a series of random <see langword="byte"/>[] data using a fast, nonsecure algorithm.
            /// </summary>
            /// <param name="length"> The length of the <see langword="byte"/>[] data. </param>
            /// <returns> The random SHA256 <see langword="byte"/>[] data. </returns>
            public static byte[] GetBytes(int length) => GetBytes(null, length);

            /// <summary>
            /// Generates a series of random <see langword="byte"/>[] data using a fast, nonsecure algorithm an additional <see langword="string"/> seed.
            /// </summary>
            /// <param name="seed"> The <see langword="int"/> seed to apply random <see langword="byte"/>[] generation. </param>
            /// <param name="length"> The length of the <see langword="byte"/>[] data. </param>
            /// <returns> The random SHA256 <see langword="byte"/>[] data. </returns>
            public static byte[] GetBytes(int seed, int length) => GetBytes((int?)seed, length);

            /// <summary>
            /// Generates a series of random <see langword="byte"/>[] data using a specific <see cref="IDigest"/> and an additional <see langword="byte"/>[] seed.
            /// </summary>
            /// <param name="seed"> The <see langword="int"/> seed to apply random <see langword="byte"/>[] generation. </param>
            /// <param name="length"> The length of the <see langword="byte"/>[] data. </param>
            /// <returns> The random <see langword="byte"/>[] data. </returns>
            private static byte[] GetBytes(int? seed, int length)
            {
                byte[] bytes;
                new System.Random(seed.Value).NextBytes(bytes = new byte[length]);
                return bytes;
            }
        }
    }
}