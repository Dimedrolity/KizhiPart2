using System;
using System.Collections.Generic;
using System.IO;

namespace KizhiPart2
{
    public class Interpreter
    {
        private readonly TextWriter _writer;

        private readonly Dictionary<string, int> _variablesInMemory = new Dictionary<string, int>();

        private readonly Dictionary<string, Action<Command>> _commandsInstructions =
            new Dictionary<string, Action<Command>>();

        private readonly Dictionary<string, List<Command>> _functions = new Dictionary<string, List<Command>>();

        private readonly List<Command> _commandsToExecute = new List<Command>();

        private bool isSourceCodeStarts;

        public Interpreter(TextWriter writer)
        {
            _writer = writer;
            SetupPrimitiveCommands();
        }

        private void SetupPrimitiveCommands()
        {
            _commandsInstructions.Add("set",
                command => _variablesInMemory[command.VariableName] = command.Value.Value);

            _commandsInstructions.Add("sub",
                command => _variablesInMemory[command.VariableName] -= command.Value.Value);

            _commandsInstructions.Add("rem",
                command => _variablesInMemory.Remove(command.VariableName));

            _commandsInstructions.Add("print",
                command => { _writer.WriteLine(_variablesInMemory[command.VariableName]); });
        }


        public void ExecuteLine(string command)
        {
            if (isSourceCodeStarts)
            {
                ParseSourceCode(command);
                isSourceCodeStarts = false;
            }

            switch (command)
            {
                case "set code":
                    isSourceCodeStarts = true;
                    break;
                case "run":
                    ExecuteCommands();
                    break;
            }
        }

        private void ParseSourceCode(string sourceCode)
        {
            var codeLines = sourceCode.Split('\n');
            var currentLineNumber = 0;

            while (currentLineNumber < codeLines.Length)
            {
                var currentLine = codeLines[currentLineNumber];
                var currentLineParts = currentLine.Split(' ');
                var commandNameOfCurrentLine = currentLineParts[0];

                switch (commandNameOfCurrentLine)
                {
                    case "def":
                    {
                        var functionName = currentLineParts[1];
                        currentLineNumber++;

                        _functions.Add(functionName, new List<Command>());

                        while (codeLines[currentLineNumber].StartsWith("    "))
                        {
                            var currentCommand = codeLines[currentLineNumber].Trim();
                            var currentCommandParts = currentCommand.Split(' ');
                            var command = ParseCommand(currentCommandParts);
                            _functions[functionName].Add(command);

                            currentLineNumber++;
                        }
                        break;
                    }
                    case "call":
                    {
                        var functionName = currentLineParts[1];
                        var function = _functions[functionName];
                        foreach (var command in function)
                        {
                            _commandsToExecute.Add(command);
                        }

                        currentLineNumber++;
                        break;
                    }
                    default:
                    {
                        var command = ParseCommand(currentLineParts);
                        _commandsToExecute.Add(command);
                        currentLineNumber++;
                        break;
                    }
                }
            }
        }

        private Command ParseCommand(string[] commandParts)
        {
            int? commandValue = null;
            if (commandParts.Length > 2)
                commandValue = int.Parse(commandParts[2]);

            return new Command(commandParts[0], commandParts[1], commandValue);
        }

        private void ExecuteCommands()
        {
            foreach (var command in _commandsToExecute)
            {
                var isSuccess = TryExecuteCommand(command);
                if (!isSuccess)
                    return;
            }
        }

        private bool TryExecuteCommand(Command command)
        {
            if (command.Name == "set" || IsVariableExists(command.VariableName))
            {
                var commandsInstruction = _commandsInstructions[command.Name];
                commandsInstruction(command);
                return true;
            }

            return false;
        }

        private bool IsVariableExists(string variableName)
        {
            if (_variablesInMemory.ContainsKey(variableName)) return true;
            _writer.WriteLine("Переменная отсутствует в памяти");
            return false;
        }

        private class Command
        {
            public string Name { get; }
            public string VariableName { get; }
            public int? Value { get; }

            public Command(string name, string variableName, int? value)
            {
                Name = name;
                VariableName = variableName;
                Value = value;
            }

            // для удобства при дебаге
            public override string ToString() => $"{Name} {VariableName} {Value}";
        }
        
        private class SourceCodeParser
        {
            private string sourceCode;
            private int currentLine;

            
            public SourceCodeParser(string sourceCode)
            {
                this.sourceCode = sourceCode;
            }

            // public List<Command> GetCommandsToExecute()
            // {
            //     
            // }
        }
    }
}