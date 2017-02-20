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
using Mono.Options;
using System.Collections.Generic;

namespace IntelHexParser
{
	/// <summary>
	/// Class to demonstrate usage of Intel HEX parser
	/// </summary>
	public class Program
	{
		static void ShowHelp (OptionSet options)
		{
			Console.WriteLine ("hex2bin v2.0");
			Console.WriteLine ("usage: hex2bin [options] <input.hex> [output.bin]");
			options.WriteOptionDescriptions (Console.Out);
		}

		static string GenerateOutputFileName (string inputFile)
		{
			return Path.Combine (Path.GetDirectoryName (inputFile), Path.GetFileNameWithoutExtension (inputFile)) + ".bin";
		}

		public static void Main (string [] args)
		{
			string rangeInput = null;
			string rangeOutput = null;

			string filling = null;
			bool minimize = false;

			bool shouldShowHelp = false;

			var options = new OptionSet {
				{"i=", "Input address or range (e.g. 0x1000:0xffff or 0x1000 or :0xffff)", i => rangeInput = i},
				{"o=", "Output address or range (e.g. 0x0000:0xffff or 0x0000 or :0xffff)", o => rangeOutput = o},
				{"f|fill=", "Filling value (default 0xff)", f => filling = f},
				{"m|minimize", "Minimize output binary - don't fill output range to upper address", m => minimize = m != null},
				{"h|help", "Show this message and exit", h => shouldShowHelp = h != null},
			};


			List<string> extra;
			try {
				// parse the command line
				extra = options.Parse (args);
			} catch (OptionException e) {
				// output some error message
				Console.Write ("hex2bin: ");
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try `hex2bin --help' for more information.");
				return;
			}

			if (shouldShowHelp || extra.Count < 1) {
				ShowHelp (options);
				return;
			}

			try {
				string fileIn = extra [0];

				string fileOut;
				if (extra.Count == 2) {
					fileOut = extra [1];
				} else {
					fileOut = GenerateOutputFileName (fileIn);
				}

				Region inputRegion = new Region (rangeInput);
				Region outputRegion = new Region (rangeOutput);
				if (minimize) {
					outputRegion.SetExactEnd (false);
				}

				byte fill;

				if (filling != null) {
					if (filling.StartsWith ("0x", StringComparison.InvariantCultureIgnoreCase)) {
						fill = Convert.ToByte (filling.Substring (2), 16);
					} else {
						fill = Convert.ToByte (filling);
					}
				} else {
					fill = 0xff;
				}


				FileStream ifs = new FileStream (fileIn, FileMode.Open);
				FileStream ofs = new FileStream (fileOut, FileMode.Create);

				// init parser
				Parser parser = new Parser (ifs);

				// 1st iteration - calculate maximum output range
				RangeDetector rangeDetector = new RangeDetector ();
				parser.SetDataListener (rangeDetector);
				parser.Parse ();
				Region fullInputRegion = rangeDetector.GetFullRangeRegion ();

				inputRegion.Intersection (fullInputRegion);

				// seek input stream back to origin
				ifs.Seek (0, SeekOrigin.Begin);

				// 2nd iteration - actual write of the output
				BinWriter writer = new BinWriter (inputRegion, outputRegion, fill, ofs);
				parser.SetDataListener (writer);
				parser.Parse ();

				// print statistics
				Console.WriteLine (string.Format ("Program start address 0x{0:x8}\r\n", parser.GetStartAddress ()));
				Console.WriteLine ("Detected memory regions: ");
				Console.WriteLine (rangeDetector.GetMemoryRegions ());

				Console.Write ("Used input region: ");
				Console.WriteLine (inputRegion);

				Console.Write ("Written output region: ");
				Console.WriteLine (writer.GetWrittenRegion());
			} catch (Exception e) {
				Console.Error.WriteLine ("Output not generated!!");
				Console.Error.WriteLine ("  " + e.Message);
			}
		}
	}
}