/**
 * Copyright (c) 2015, Jan Breuer All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * * Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace IntelHex
{
	/// <summary>
	/// One memory region
	/// </summary>
	public class Region : IComparable
	{

		uint addressStart;
		uint addressEnd;
		bool exactStart;
		bool exactEnd;

		public Region (string range)
		{
			string [] region = range == null ? new string [0] : range.Split (':');
			if (region.Length == 0) {
				exactStart = ReadAddress ("min", out addressStart);
				exactEnd = ReadAddress ("max", out addressEnd);
			} else if (region.Length == 1) {
				exactStart = ReadAddress (region [0], out addressStart);
				exactEnd = ReadAddress ("max", out addressEnd);
			} else {
				exactStart = ReadAddress (region [0], out addressStart);
				exactEnd = ReadAddress (region [1] != null && region [1] != "" ? region [1] : "max", out addressEnd);
			}
		}

		public Region (uint start, uint length)
		{
			addressStart = start;
			addressEnd = start + length - 1;
			exactStart = true;
			exactEnd = true;
		}

		static bool ReadAddress (string s, out uint result)
		{
			if (s != null && s != "") {
				if (s == "min") {
					result = 0;
					return false;
				}
				if (s == "max") {
					result = 0xFFFFFFFF;
					return false;
				}
				if (s.StartsWith ("0x", StringComparison.InvariantCultureIgnoreCase)) {
					result = Convert.ToUInt32 (s.Substring (2), 16);
				} else {
					result = Convert.ToUInt32 (s);
				}
				return true;
			}
			result = 0;
			return false;
		}


		/// <summary>
		/// Get length of the region
		/// </summary>
		/// <returns>The length.</returns>
		public uint GetLength ()
		{
			if (addressStart <= addressEnd) {
				return addressEnd - addressStart + 1;
			}

			return 0;
		}

		/// <summary>
		/// Return last address in memory region
		/// </summary>
		/// <returns>The address end.</returns>
		public long GetAddressEnd ()
		{
			return addressEnd;
		}

		/// <summary>
		/// Set end address
		/// </summary>
		/// <param name="addressEnd">Address end.</param>
		public void SetAddressEnd (uint addressEnd)
		{
			this.addressEnd = addressEnd;
			this.exactEnd = true;
		}

		/// <summary>
		/// Get start address of the region
		/// </summary>
		/// <returns>The address start.</returns>
		public uint GetAddressStart ()
		{
			return addressStart;
		}

		/// <summary>
		/// Set start address
		/// </summary>
		/// <param name="addressStart">Address start.</param>
		public void SetAddressStart (uint addressStart)
		{
			this.addressStart = addressStart;
			this.exactStart = true;
		}

		/// <summary>
		/// Increment length of the region by value
		/// </summary>
		/// <param name="value">Value.</param>
		public void IncLength (uint value)
		{
			addressEnd += value;
		}


		public bool HasExactStart ()
		{
			return exactStart;
		}

		public void SetExactStart (bool value)
		{
			exactStart = value;
		}

		/// <summary>
		/// Hases the exact end.
		/// </summary>
		/// <returns>Wether the end address was exactly defined or calculated</returns>
		public bool HasExactEnd ()
		{
			return exactEnd;
		}

		public void SetExactEnd (bool value)
		{
			exactEnd = value;
		}

		public void Intersection (Region other)
		{
			addressStart = addressStart > other.addressStart ? addressStart : other.addressStart;
			addressEnd = addressEnd < other.addressEnd ? addressEnd : other.addressEnd;
			//exactStart = exactStart | other.exactStart;
			//exactEnd = exactEnd | other.exactEnd;
		}

		/// <summary>
		/// Convert region to string
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="IntelHex.Region"/>.</returns>
		public override string ToString ()
		{
			return String.Format ("0x{0:x8}:0x{1:x8} ({2}B 0x{3:x8})", addressStart, addressEnd, GetLength (), GetLength ());
		}


		/// <summary>
		/// Compare, if one region is after another region
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="o">O.</param>
		public int CompareTo (object o)
		{
			Region r = (Region)o;
			if (this.addressStart == r.addressStart) {
				return this.addressEnd.CompareTo(r.addressEnd);
			} else {
				return this.addressStart.CompareTo(r.addressStart);
			}
		}
	}
}