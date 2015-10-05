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
using IntelHex.Listeners;


namespace IntelHexParser
{
	/// <summary>
	/// Class to demonstrate usage of Intel HEX parser
	/// </summary>
	public class Program
	{
		public Program ()
		{
		}

		/// <summary>
     	/// Convert Intel HEX to bin
		///
		/// usage:
		///
		/// IntelHexParserDemo {source} {target}
		///
		/// IntelHexParserDemo {source} {target} {address_from} {address_to}
		/// 
		/// {source} is source Intel HEX file name
		///
		/// {target} is target BIN file name
		///
		/// {address_from} is start address e.g. 0x1D000000 or min
		///
		/// {address_to} is end address e.g. 0x1D07FFFF or max
		///
		/// if no address_from and address_to is specified, maximum range is used
		///
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args)
		{
			string fileIn = "Application.hex";
			string fileOut = "Application.bin";
			string dataFrom = "min";
			string dataTo = "max";
			bool minimize = false;

			if (args.Length == 0) {
				Console.WriteLine ("usage:");
				Console.WriteLine ("    hex2bin <hex> <bin> <start address> <end address> [minimize]");            
				Console.WriteLine ();
				Console.WriteLine ("    full address range of app.hex");
				Console.WriteLine ("        hex2bin app.hex app.bin");
				Console.WriteLine ();
				Console.WriteLine ("    limited exact address range of app.hex, undefined data are 0xff");
				Console.WriteLine ("        hex2bin app.hex app.bin 0x0000 0x1fff");
				Console.WriteLine ();
				Console.WriteLine ("    limited minimal address range of app.hex, start at 0x0000,");
				Console.WriteLine ("    max address is 0x1fff, but can be lower");
				Console.WriteLine ("        hex2bin app.hex app.bin 0x0000 0x1fff minimize");
				return;
			}

			if (args.Length >= 1) {
				fileIn = args [0];
			}

			if (args.Length >= 2) {
				fileOut = args [1];
			}

			if (args.Length >= 3) {
				dataFrom = args [2];
			}

			if (args.Length >= 4) {
				dataTo = args [3];
			}

			if (args.Length >= 5) {
				if (args [4].Equals ("minimize")) {
					minimize = true;
				}
			}

			FileStream ifs = new FileStream (fileIn, FileMode.Open);
			FileStream ofs = new FileStream (fileOut, FileMode.Create);
			// init parser
			Parser parser = new Parser (ifs);

			// 1st iteration - calculate maximum output range            
			RangeDetector rangeDetector = new RangeDetector ();
			parser.SetDataListener (rangeDetector);
			parser.Parse ();

			ifs.Seek (0, SeekOrigin.Begin);

			Region outputRegion = rangeDetector.GetFullRangeRegion ();

			// if address parameter is "max", calculate maximum memory region
			if (!("min".Equals (dataFrom))) {
				outputRegion.SetAddressStart (Convert.ToUInt32 (dataFrom.Substring (2), 16));
			} 
			if (!("max".Equals (dataTo))) {
				outputRegion.SetAddressEnd (Convert.ToUInt32 (dataTo.Substring (2), 16));
			}

			// 2nd iteration - actual write of the output
			BinWriter writer = new BinWriter (outputRegion, ofs, minimize);
			parser.SetDataListener (writer);
			parser.Parse ();

			// print statistics
			Console.WriteLine (string.Format("Program start address 0x{0:x8}{{0}}\r\n", parser.GetStartAddress ()));
			Console.WriteLine ("Memory regions: ");
			Console.WriteLine (writer.getMemoryRegions ());

			Console.Write ("Written output: ");
			Console.WriteLine (outputRegion);


		}

	}
}