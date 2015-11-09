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
using IntelHex;

namespace IntelHex
{
	/// <summary>
	/// Class to hold one Intel HEX record - one line in the file
	/// </summary>
	public class Record
	{
		public uint Length;
		public uint Address;
		public RecordType Type;
		public byte[] Data;

		/// <summary>
		/// Convert record to string for debuging
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="IntelHex.Record"/>.</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			sb.Append(Type);
			sb.Append(" @");
			sb.Append(string.Format("0x{0:x4}", Address));
			sb.Append(" [");
			foreach (byte c in Data) {
				sb.Append(string.Format("{0:x2}", c));
				sb.Append(" ");
			}
			sb.Remove(sb.Length - 1, 1);
			sb.Append("]");
			return sb.ToString();
		}
	}
}

