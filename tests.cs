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
            var sw = new StreamWriter(@"D:\testSimple.txt");
            sw.AutoFlush = true;
            var interpreter = new Interpreter(sw);
            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("set a 10\nprint a\nprint a\nsub a 4\nprint a\nrem a");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");
        }

        [Test]
        public void AfterRem()
        {
            var sw = new StreamWriter(@"D:\testAfterRem.txt");
            sw.AutoFlush = true;

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("set a 10\nrem a\nprint a\nset b 20\nprint b");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");
        }

        [Test]
        public void SimpleFunc()
        {
            var sw = new StreamWriter(@"D:\testmyfuc.txt");
            sw.AutoFlush = true;

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("def myfunc\n    set a 10\n    print a\ncall myfunc\nprint a");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");
        }


        [Test]
        public void ExampleFunc()
        {
            var sw = new StreamWriter(@"D:\testexfunc.txt");
            sw.AutoFlush = true;

            var interpreter = new Interpreter(sw);

            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine("def test\n    set a 10\n    sub a 3\n    print b\nset b 7\ncall test");
            interpreter.ExecuteLine("end code");
            interpreter.ExecuteLine("run");
        }
    }
}