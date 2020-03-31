using System;
using System.IO;
using NUnit.Framework;

namespace KizhiPart2
{
    [TestFixture]
    public class InterpreterTests
    {
        private readonly string[] _commandsSeparator = {"\r\n"};

        void TestInterpreter(string[] commands, string[] expectedOutput)
        {
            string[] actualOutput;
            using (var sw = new StringWriter())
            {
                var interpreter = new Interpreter(sw);

                foreach (var command in commands)
                {
                    interpreter.ExecuteLine(command);
                }

                actualOutput = sw.ToString().Split(_commandsSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

            Assert.AreEqual(expectedOutput.Length, actualOutput.Length);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void SeveralCommands()
        {
            TestInterpreter(new[]
            {
                "set code",

                "set a 10\n" +
                "print a\n" +
                "print a\n" +
                "sub a 4\n" +
                "print a\n" +
                "rem a",

                "end code",
                "run"
            }, new[] {"10", "10", "6"});
        }

        [Test]
        public void DontExecuteAfterNotFound()
        {
            TestInterpreter(new[]
            {
                "set code",

                "print a\n" +
                "set a 5\n" +
                "print a",

                "end code",
                "run"
            }, new[] {"Переменная отсутствует в памяти"});
        }

        [Test]
        public void SimpleFunction()
        {
            TestInterpreter(new[]
            {
                "set code",

                "def myfunc\n" +
                "    set a 10\n" +
                "    print a\n" +
                "call myfunc\n" +
                "print a",

                "end code",
                "run"
            }, new[] {"10", "10"});
        }

        [Test]
        public void FunctionFromExample()
        {
            TestInterpreter(new[]
            {
                "set code",

                "def test\n" +
                "    set a 10\n" +
                "    sub a 3\n" +
                "    print b\n" +
                "set b 7\n" +
                "call test",

                "end code",
                "run"
            }, new[] {"7"});
        }

        [Test]
        public void FunctionCallBeforeDefinition()
        {
            TestInterpreter(new[]
            {
                "set code",

                "call test\n" +
                "print a\n" +
                "def test\n" +
                "    set a 5",

                "end code",
                "run"
            }, new[] {"5"});
        }

        [Test]
        public void VariableValueIsZero()
        {
            Assert.Throws<ArgumentException>(() =>
                TestInterpreter(new[]
                {
                    "set code",

                    "set m 0",

                    "end code",
                    "run"
                }, new string[0])
            );
        }

        [Test]
        public void FunctionCallFunction()
        {
            TestInterpreter(new[]
            {
                "set code",
                "def one\n" +
                "    set a 20\n" +
                "    sub a 5\n" +
                "    call two\n" +
                "    print a\n" +
                "def two\n" +
                "    sub a 5\n" +
                "    sub a 5\n" +
                "call one",
                "end code",
                "run"
            }, new[] {"5"});
        }

        [Test]
        public void RunAndRunAfterResetBuffers()
        {
            TestInterpreter(new[]
            {
                "set code",

                "def test\n" +
                "    set a 5\n" +
                "    sub a 3\n" +
                "    print a\n" +
                "call test",

                "end code",
                "run",
                "run"
            }, new[] {"2", "2"});
        }

        [Test]
        public void RunCodeWithErrorAndRunAfterResetBuffers()
        {
            TestInterpreter(new[]
                {
                    "set code",

                    "sub a\n" +
                    "set a 4\n" +
                    "print a",

                    "end code",
                    "run",
                    "run"
                },
                new[] {"Переменная отсутствует в памяти", "Переменная отсутствует в памяти"});
        }


        //WORKS
        // [Test]
        // public void Recursive()
        // {
        //     TestInterpreter(new[]
        //     {
        //         "set code",
        //         
        //         "def printfunc\n" +
        //         "    print a\n" +
        //         "    call printfunc\n" +
        //         "set a 5\n" +
        //         "call printfunc\n",
        //         
        //         "end code",
        //         "run"
        //     }, new string[0]);
        // }
    }
}