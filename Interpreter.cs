using System;
using System.Collections.Generic;
using System.IO;

namespace KizhiPart2
{
    public class Interpreter
    {
        private readonly TextWriter _writer;

        private readonly Dictionary<string, int> _memoryVariables = new Dictionary<string, int>();

        private readonly Dictionary<string, Action<PrimitiveCommand>> _commandsActions =
            new Dictionary<string, Action<PrimitiveCommand>>();

        private bool _sourceCodeStarts;

        private SourceCodeParser _sourceCodeParser;

        public Interpreter(TextWriter writer)
        {
            _writer = writer;
            SetupPrimitiveCommands();
        }

        private void SetupPrimitiveCommands()
        {
            _commandsActions.Add("set",
                command => _memoryVariables[command.VariableName] = command.Value.Value);

            _commandsActions.Add("sub",
                command => _memoryVariables[command.VariableName] -= command.Value.Value);

            _commandsActions.Add("rem",
                command => _memoryVariables.Remove(command.VariableName));

            _commandsActions.Add("print",
                command => _writer.WriteLine(_memoryVariables[command.VariableName]));
        }

        public void ExecuteLine(string command)
        {
            if (_sourceCodeStarts)
            {
                _sourceCodeParser = new SourceCodeParser(command);
                _sourceCodeStarts = false;
            }

            switch (command)
            {
                case "set code":
                    _sourceCodeStarts = true;
                    break;
                case "run":
                    var primitiveCommands = _sourceCodeParser.GetCommandsToExecute();
                    Execute(primitiveCommands);
                    break;
            }
        }

        private void Execute(List<PrimitiveCommand> primitiveCommands)
        {
            foreach (var command in primitiveCommands)
            {
                bool success = TryExecute(command);
                if (!success)
                    return;
            }
        }

        private bool TryExecute(PrimitiveCommand primitiveCommand)
        {
            if (primitiveCommand.Name == "set" || IsMemoryContains(primitiveCommand.VariableName))
            {
                var commandAction = _commandsActions[primitiveCommand.Name];
                commandAction(primitiveCommand);
                return true;
            }

            return false;
        }

        private bool IsMemoryContains(string variableName)
        {
            if (_memoryVariables.ContainsKey(variableName)) return true;

            _writer.WriteLine("Переменная отсутствует в памяти");
            return false;
        }

        private class PrimitiveCommand
        {
            public string Name { get; }
            public string VariableName { get; }
            public int? Value { get; }

            public PrimitiveCommand(string name, string variableName, int? value)
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
            private readonly string _sourceCode;
            private string[] _codeLines;
            private int _currentLineNumber;
            private string[] _currentLineParts;

            private readonly List<PrimitiveCommand> _commandsSequenceToExecute = new List<PrimitiveCommand>();

            private readonly Dictionary<string, List<PrimitiveCommand>> _functions =
                new Dictionary<string, List<PrimitiveCommand>>();

            public SourceCodeParser(string sourceCode)
            {
                _sourceCode = sourceCode;
            }

            public List<PrimitiveCommand> GetCommandsToExecute()
            {
                _codeLines = _sourceCode.Split('\n');

                while (_currentLineNumber < _codeLines.Length)
                {
                    var currentLine = _codeLines[_currentLineNumber];
                    _currentLineParts = currentLine.Split(' ');
                    var commandNameOfCurrentLine = _currentLineParts[0];

                    ParseCommandWithName(commandNameOfCurrentLine);
                }

                return _commandsSequenceToExecute;
            }

            private void ParseCommandWithName(string commandName)
            {
                switch (commandName)
                {
                    case "def":
                        TakeCommandsFromFunctionBody();
                        break;
                    case "call":
                        AddFunctionCommandsToExecutionList();
                        break;
                    default:
                        AddPrimitiveCommandToExecutionList();
                        break;
                }
            }

            private void TakeCommandsFromFunctionBody()
            {
                var functionName = _currentLineParts[1];
                _functions.Add(functionName, new List<PrimitiveCommand>());

                _currentLineNumber++;

                while (_codeLines[_currentLineNumber].StartsWith("    "))
                {
                    var currentCommand = _codeLines[_currentLineNumber].TrimStart();
                    var currentCommandParts = currentCommand.Split(' ');
                    var command = CreateCommandFrom(currentCommandParts);
                    _functions[functionName].Add(command);

                    _currentLineNumber++;
                }
            }

            private PrimitiveCommand CreateCommandFrom(string[] commandParts)
            {
                int? commandValue = null;
                if (commandParts.Length > 2)
                    commandValue = int.Parse(commandParts[2]);

                return new PrimitiveCommand(commandParts[0], commandParts[1], commandValue);
            }

            private void AddFunctionCommandsToExecutionList()
            {
                var functionName = _currentLineParts[1];
                var functionCommands = _functions[functionName];
                foreach (var command in functionCommands)
                {
                    _commandsSequenceToExecute.Add(command);
                }

                _currentLineNumber++;
            }
            
            private void AddPrimitiveCommandToExecutionList()
            {
                var command = CreateCommandFrom(_currentLineParts);
                _commandsSequenceToExecute.Add(command);
                
                _currentLineNumber++;
            }
        }
    }
}