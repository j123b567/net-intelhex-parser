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
using System.IO;
using IntelHex;
using System.Linq;


namespace IntelHex.Listeners
{
	/// <summary>
	/// Binary file writer
	/// </summary>
	public class BinWriter : IDataListener
	{


		private Region outputRegion;
		private Stream destination;
		private byte[] buffer;
		private MemoryRegions regions;
		private uint maxAddress;
		private bool minimize;

		public BinWriter (Region outputRegion, Stream destination, bool minimize)
		{
			this.outputRegion = outputRegion;
			this.destination = destination;
			this.minimize = minimize;
			this.buffer = Enumerable.Repeat((byte)0xFF, (int)outputRegion.GetLength ()).ToArray();
			regions = new MemoryRegions ();
			maxAddress = outputRegion.GetAddressStart ();
		}

		public void Data (uint address, byte[] data)
		{
			regions.Add (address, (uint)data.Length);

			if ((address >= outputRegion.GetAddressStart ()) && (address <= outputRegion.GetAddressEnd ())) {
				int length = data.Length;
				if ((address + length) > outputRegion.GetAddressEnd ()) {
					length = (int)(outputRegion.GetAddressEnd () - address + 1);
				}
				Array.Copy (data, 0, buffer, (int)(address - outputRegion.GetAddressStart ()), length);

				if (maxAddress < (address + data.Length - 1)) {
					maxAddress = address + (uint)data.Length - 1;
				}
			}
		}

		public void Eof ()
		{       
			if (!minimize) {
				maxAddress = (uint)outputRegion.GetAddressEnd ();
			}
			destination.Write (buffer, 0, (int)(maxAddress - outputRegion.GetAddressStart () + 1));
		}

		public MemoryRegions GetMemoryRegions ()
		{
			return regions;
		}
	}
}