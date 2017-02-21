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

		Region inputRegion;
		Region outputRegion;

		Stream destination;
		byte[] buffer;
		MemoryRegions writtenRegions;
		Region written;


		public BinWriter (Region inputRegion, Region outputRegion, byte fill, Stream destination)
		{
			this.inputRegion = inputRegion;
			this.outputRegion = outputRegion;
			this.destination = destination;

			writtenRegions = new MemoryRegions ();
			written = new Region (0, 0);

			if (!this.outputRegion.HasExactStart ()) {
				this.outputRegion.SetAddressStart (this.inputRegion.GetAddressStart ());
			}
			if (!this.outputRegion.HasExactEnd ()) {
				this.outputRegion.SetAddressEnd ((uint)this.inputRegion.GetAddressEnd ());
			}

			buffer = Enumerable.Repeat (fill, (int)outputRegion.GetLength ()).ToArray ();
		}

		public void Data (uint address, byte[] data)
		{
			var current = new Region (address, (uint)data.Length);
			current.Intersection (inputRegion);
			current.Intersection (outputRegion);

			if (current.GetLength () > 0) {
				long sourceIndex = current.GetAddressStart () - address;
				long destinationIndex = address - outputRegion.GetAddressStart();
				Array.Copy (data, (int)sourceIndex, buffer, (int)destinationIndex, current.GetLength());
				writtenRegions.Add (current);
			}
		}

		public void Eof ()
		{
			UpdateWritten ();
			destination.Write (buffer, 0, (int)written.GetLength());
		}

		void UpdateWritten ()
		{
			written = writtenRegions.GetFullRangeRegion ();
			if (outputRegion.HasExactStart ()) {
				written.SetAddressStart (outputRegion.GetAddressStart ());
			}
			if (outputRegion.HasExactEnd ()) {
				written.SetAddressEnd ((uint)outputRegion.GetAddressEnd ());
			}
		}

		public Region GetWrittenRegion ()
		{
			return written;
		}
	}
}