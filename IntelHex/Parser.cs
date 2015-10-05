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

namespace IntelHex
{
	public class Parser
	{
		private StreamReader reader;
		private IDataListener dataListener = null;
		private bool eof = false;
		private int recordIdx = 0;
		private uint upperAddress = 0;
		private uint startAddress = 0;

		/// <summary>
		/// Constructor of the parser with reader
		/// </summary>
		/// <param name="st">Stream.</param>
		public Parser (Stream st)
		{
			this.reader = new StreamReader(new BufferedStream (st));
		}

		/// <summary>
		/// Set data listener to parsing events (data and eof)
		/// </summary>
		/// <param name="listener">Listener.</param>
		public void SetDataListener (IDataListener listener)
		{
			this.dataListener = listener;
		}


		/// <summary>
		/// Parse one line of Intel HEX file
		/// </summary>
		/// <returns>The record.</returns>
		/// <param name="record">Record.</param>
		private Record ParseRecord (string record)
		{
			Record result = new Record ();
			// check, if there wasn an accidential EOF record
			if (eof) {
				throw new IntelHexException ("Data after eof (" + recordIdx + ")");
			}

			// every IntelHEX record must start with ":"
			if (!record.StartsWith (":")) {
				throw new IntelHexException ("Invalid Intel HEX record (" + recordIdx + ")");
			}

			int lineLength = record.Length;
			byte[] hexRecord = new byte[lineLength / 2];

			// sum of all bytes modulo 256 (including checksum) shuld be 0
			int sum = 0;
			for (int i = 0; i < hexRecord.Length; i++) {
				String num = record.Substring (2 * i + 1, 2);
				hexRecord [i] = Convert.ToByte (num, 16);
				sum += hexRecord [i] & 0xff;
			}
			sum &= 0xff;

			if (sum != 0) {
				throw new IntelHexException ("Invalid checksum (" + recordIdx + ")");
			}

			// if the length field does not correspond with line length
			result.Length = hexRecord [0];
			if ((result.Length + 5) != hexRecord.Length) {
				throw new IntelHexException ("Invalid record length (" + recordIdx + ")");
			}
			// length is OK, copy data
			result.Data = new byte[result.Length];
			Array.Copy (hexRecord, 4, result.Data, 0, result.Length);

			// build lower part of data address
			result.Address = (uint)((hexRecord [1] & 0xFF) << 8) | (uint)(hexRecord [2] & 0xFF);

			// determine record type
			result.Type = (RecordType)(hexRecord [3] & 0xFF);
			if (!Enum.IsDefined(typeof(RecordType), result.Type)) {
				throw new IntelHexException ("Unsupported record type " + (hexRecord [3] & 0xFF) + " (" + recordIdx + ")");
			}

			return result;
		}

		/// <summary>
		/// Process parsed record, copute correct address, emit events
		/// </summary>
		/// <param name="record">Record.</param>
		private void ProcessRecord (Record record)
		{
			// build full address
			uint addr = record.Address | upperAddress;
			switch (record.Type) {
			case RecordType.DATA:
				if (dataListener != null) {
					dataListener.Data (addr, record.Data);
				}
				break;
			case RecordType.EOF:
				if (dataListener != null) {
					dataListener.Eof ();
				}
				eof = true;
				break;
			case RecordType.EXT_LIN:
				if (record.Length == 2) {
					upperAddress = (uint)((record.Data [0] & 0xFF) << 8) + (uint)(record.Data [1] & 0xFF);
					upperAddress <<= 16; // ELA is bits 16-31 of the segment base address (SBA), so shift left 16 bits
				} else {
					throw new IntelHexException ("Invalid EXT_LIN record (" + recordIdx + ")");
				}

				break;
			case RecordType.EXT_SEG:
				if (record.Length == 2) {
					upperAddress = (uint)((record.Data [0] & 0xFF) << 8) + (uint)(record.Data [1] & 0xFF);
					upperAddress <<= 4; // ESA is bits 4-19 of the segment base address (SBA), so shift left 4 bits
				} else {
					throw new IntelHexException ("Invalid EXT_SEG record (" + recordIdx + ")");
				}
				break;
			case RecordType.START_LIN:
				if (record.Length == 4) {
					startAddress = 0;
					foreach (byte c in record.Data) {
						startAddress = startAddress << 8;
						startAddress |= (uint)(c & 0xFF);
					}
				} else {
					throw new IntelHexException ("Invalid START_LIN record at line #" + recordIdx + " " + record);
				}
				break;
			case RecordType.START_SEG:
				if (record.Length == 4) {
					startAddress = 0;
					foreach (byte c in record.Data) {
						startAddress = startAddress << 8;
						startAddress |= (uint)(c & 0xFF);
					}
				} else {
					throw new IntelHexException ("Invalid START_SEG record at line #" + recordIdx + " " + record);
				}
				break;
			case RecordType.UNKNOWN:
				break;
			}

		}

		/// <summary>
		/// Return program start address/reset address. May not be at the beggining 
		/// of the data.
		/// </summary>
		/// <returns>The start address.</returns>
		public long GetStartAddress ()
		{
			return startAddress;
		}

		/// <summary>
		/// Main public method to start parsing of the input
		/// </summary>
		public void Parse ()
		{
			eof = false;
			recordIdx = 1;
			upperAddress = 0;
			startAddress = 0;
			String recordStr;

			while ((recordStr = reader.ReadLine()) != null) {
				Record record = ParseRecord (recordStr);
				ProcessRecord (record);
				recordIdx++;
			}

			if (!eof) {
				throw new IntelHexException ("No eof at the end of file");
			}
		}
	}
}