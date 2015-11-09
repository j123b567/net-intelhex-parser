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

		private uint addressStart;
		private uint addressEnd;

		public Region (uint start, uint length)
		{
			this.addressStart = start;
			this.addressEnd = start + length - 1;
		}

		/// <summary>
		/// Get length of the region
		/// </summary>
		/// <returns>The length.</returns>
		public uint GetLength ()
		{
			return addressEnd - addressStart + 1;
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
		}

		/// <summary>
		/// Increment length of the region by value
		/// </summary>
		/// <param name="value">Value.</param>
		public void IncLength (uint value)
		{
			addressEnd += value;
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