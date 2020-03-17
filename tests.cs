using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace KizhiPart2
{
    [TestFixture]
    public class tests
    {
        [Test]
        public void SimpleTest()
        {
            var path = @"D:\testSimple.txt";
            var sw = new StreamWriter(path) {AutoFlush = true};
            var interpreter = new Interpreter(sw);
            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("set a 10\nprint a\nprint a\nsub a 4\nprint a\nrem a");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();
            var line3 = sr.ReadLine();
            var line4 = sr.ReadLine();
            Assert.AreEqual(new[] {"10", "10", "6", null},
                new[] {line, line2, line3, line4});
        }

        [Test]
        public void AfterRem()
        {
            var path = @"D:\testAfterRem.txt";
            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("set a 10\nrem a\nprint a\nset b 20\nprint b");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();

            Assert.AreEqual(new[] {"Переменная отсутствует в памяти", null}, new[] {line, line2});
        }

        [Test]
        public void SimpleFunc()
        {
            var path = @"D:\testmyfuc.txt";

            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("def myfunc\n    set a 10\n    print a\ncall myfunc\nprint a");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();
            var line3 = sr.ReadLine();

            Assert.AreEqual(new[] {"10", "10", null},
                new[] {line, line2, line3});
        }


        [Test]
        public void ExampleFunc()
        {
            var path = @"D:\testexfunc.txt";

            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("def test\n    set a 10\n    sub a 3\n    print b\nset b 7\ncall test");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();

            Assert.AreEqual(new[] {"7", null}, new[] {line, line2});
        }

        [Test]
        public void DefinitionBelowCall()
        {
            var path = @"D:\testDefBelowCall.txt";

            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("call test\nprint a\ndef test\n    set a 5");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();


            Assert.AreEqual(new[] {"5", null},
                new[] {line, line2,});
        }

        [Test]
        public void DontExecuteAfterNotFound()
        {
            var path = @"D:\testDontExecuteAfterNotFound.txt";
            var sw = new StreamWriter(path) {AutoFlush = true};
            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine(
                "print a\n" +
                "set a 5\n" +
                "print a\n");
            interpreter.ExecuteLine("end code");

            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();
            Assert.AreEqual(new[] {"Переменная отсутствует в памяти", null},
                new[] {line, line2});
        }

        [Test]
        public void VariableValueIsZero()
        {
            var path = @"testVariableValueIsZero.txt";
            var sw = new StreamWriter(path) {AutoFlush = true};
            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("set m 0");
            interpreter.ExecuteLine("end code");

            Assert.Throws<ArgumentException>(() => interpreter.ExecuteLine("run"));
            sw.Close();
        }

        [Test]
        public void FuncCallFunc()
        {
            var path = @"D:\testFuncCallFunc.txt";

            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine(
                "def one\n" +
                "    set a 20\n" +
                "    sub a 5\n" +
                "    call two\n" +
                "    print a\n" +
                "def two\n" +
                "    sub a 5\n" +
                "    sub a 5\n" +
                "call one");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");


            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();


            Assert.AreEqual(new[] {"5", null},
                new[] {line, line2});
        }

        [Test]
        public void RunAndRunAfterResetBuffers()
        {
            var path = @"D:\testRunAndRunAfterResetBuffers.txt";

            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("def test\n    set a 5\n    sub a 3\n    print a\ncall test");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();
            var line3 = sr.ReadLine();


            Assert.AreEqual(new[] {"2", "2", null},
                new[] {line, line2, line3,});
        }

        [Test]
        public void RunAndRunAfterResetBuffersWithError()
        {
            var path = @"D:\testRunAndRunAfterResetBuffersWithError.txt";

            var sw = new StreamWriter(path) {AutoFlush = true};

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("sub a\nset a 4");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");
            interpreter.ExecuteLine("run");

            sw.Close();
            var sr = new StreamReader(path);
            var line = sr.ReadLine();
            var line2 = sr.ReadLine();
            var line3 = sr.ReadLine();


            Assert.AreEqual(new[] {"Переменная отсутствует в памяти", "Переменная отсутствует в памяти", null},
                new[] {line, line2, line3,});
        }


        //WORKS
        // [Test]
        // public void Recursive()
        // {
        //     var path = @"D:\testRecursive.txt";
        //
        //     var sw = new StreamWriter(path);
        //     sw.AutoFlush = true;
        //
        //     var interpreter = new Interpreter(sw);
        //
        //     interpreter.ExecuteLine("set code");
        //     interpreter.ExecuteLine(
        //         "def printfunc\n" +
        //         "    print a\n" +
        //         "    call printfunc\n" +
        //         "set a 5\n" +
        //         "call printfunc\n");
        //     interpreter.ExecuteLine("end code");
        //     interpreter.ExecuteLine("run");
        // }
    }
}