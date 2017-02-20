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
using System.Text;
using System.Collections.Generic;

namespace IntelHex
{
	/// <summary>
	/// Class to hold all memory address regions
	/// </summary>
	public class MemoryRegions
	{

		private List<Region> regions = new List<Region> ();

		/// <summary>
		/// Add region to list
		/// </summary>
		/// <param name="region">Region.</param>
		public void Add (Region region)
		{
			Add (region.GetAddressStart (), region.GetLength ());
		}

		/// <summary>
		/// Add region to list
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="length">Length.</param>
		public void Add (uint start, uint length)
		{
			Region prevRegion;
			if (regions.Count > 0) {
				prevRegion = regions[regions.Count - 1];
				uint nextAddress = prevRegion.GetAddressStart () + prevRegion.GetLength ();
				if (nextAddress == start) {
					prevRegion.IncLength (length);
					return;
				}
			}
			regions.Add (new Region (start, length));
		}

		/// <summary>
		/// Compact regions
		/// </summary>
		public void Compact ()
		{
			regions.Sort ();

			Region prev = null;
			for(int i = 0; i < regions.Count; i++) {
				Region curr = regions[i];
				if (prev == null) {
					prev = curr;
				} else {
					// check for chaining
					if (curr.GetAddressStart () == (prev.GetAddressStart () + prev.GetLength ())) {
						prev.IncLength (curr.GetLength ());
						regions.RemoveAt (i--);
					} else {
						prev = curr;
					}
				}
			}
		}

		/// <summary>
		/// Clear all regions
		/// </summary>
		public void Clear ()
		{
			regions.Clear ();
		}

		/// <summary>
		/// Number of regions
		/// </summary>
		public int Size ()
		{
			return regions.Count;
		}

		/// <summary>
		/// Get specific region
		/// </summary>
		/// <param name="index">Index.</param>
		public Region Get (int index)
		{
			return regions[index];
		}

		/// <summary>
		/// Get region representing whole memory
		/// </summary>
		/// <returns>The full range region.</returns>
		public Region GetFullRangeRegion ()
		{
			uint start = 0;
			uint length = 0;
			if (regions.Count != 0) {
				start = regions[0].GetAddressStart ();
				Region last = regions[regions.Count - 1];
				length = last.GetAddressStart () + last.GetLength () - start;
			}

			return new Region (start, length);
		}

		/// <summary>
		/// String representing region
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="IntelHex.MemoryRegions"/>.</returns>
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (Region r in regions) {
				sb.Append (r).Append ("\r\n");
			}

			return sb.ToString ();
		}
	}
}