.NET IntelHex Parser Library
--------

* IntelHex file format parsing library written in Java.
* Licensed under Simplified BSD license
* Including demo code: intelhex to binary converter hex2bin

~~~~~c#

    class MyDataListener : IDataListener {
        public void Data(uint address, byte[] data) {
            // process data
        }
        
        public void Eof() {
            // do some action
        }
    }

    // create input stream of some IntelHex data
    IFileStream ifs = new FileStream ("Application.hex", FileMode.Open);
    
    // create IntelHex Parser object
    Parser parser = new Parser (ifs);
    IDataListener dataListener = new MyDataListener();
    
    // register parser listener
    parser.SetDataListener(dataListener);
    parser.Parse();
~~~~~


There are two predefined `IDataListener` classes - `RangeDetector` and `BinWriter`. First can be used to detect memory ranges in intelhex file, second to write data to binary file.