using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KizhiPart2
{
    public class Interpreter
    {
        private bool _isSourceCodeStarts;

        private string[] _codeLines;
        private int _currentLineNumber;
        private string[] _currentLineParts;

        private readonly Dictionary<string, int> _functionNameToDefinitionLine = new Dictionary<string, int>();

        private readonly Stack<(string FunctionName, int CallLine)> _callStack = new Stack<(string, int)>();

        private Command _commandForExecute;
        private readonly CommandExecutor _commandExecutor;

        public Interpreter(TextWriter writer)
        {
            _commandExecutor = new CommandExecutor(writer);
        }

        public void ExecuteLine(string command)
        {
            if (_isSourceCodeStarts)
            {
                _codeLines = command.Split('\n').Where(line => !string.IsNullOrEmpty(line)).ToArray();
                FindAllFunctionDefinitions();
                _isSourceCodeStarts = false;
            }

            switch (command)
            {
                case "set code":
                    _isSourceCodeStarts = true;
                    break;
                case "run":
                    StartInterpretation();
                    break;
            }
        }

        private void FindAllFunctionDefinitions()
        {
            while (_currentLineNumber < _codeLines.Length)
            {
                if (_codeLines[_currentLineNumber].StartsWith("def"))
                {
                    _currentLineParts = _codeLines[_currentLineNumber].Split(' ');
                    var functionName = _currentLineParts[1];
                    _functionNameToDefinitionLine.Add(functionName, _currentLineNumber);
                }

                _currentLineNumber++;
            }

            _currentLineNumber = 0;
        }

        private void StartInterpretation()
        {
            var isPreviousCommandExecuted = true;

            while (_currentLineNumber < _codeLines.Length && isPreviousCommandExecuted)
            {
                ParseCurrentCodeLine();

                if (_commandForExecute != null)
                    isPreviousCommandExecuted = _commandExecutor.TryExecute(_commandForExecute);

                _commandForExecute = null;
            }
        }

        private void ParseCurrentCodeLine()
        {
            if (_callStack.Count != 0)
                ParseCurrentLineOfFunction();
            else
                ParseCommandOfCurrentLine();
        }

        private void ParseCurrentLineOfFunction()
        {
            var functionName = _callStack.Peek().FunctionName;
            var definitionLine = _functionNameToDefinitionLine[functionName];

            if (_currentLineNumber == definitionLine)
            {
                _currentLineNumber++;
            }
            else if (_currentLineNumber < _codeLines.Length && _codeLines[_currentLineNumber].StartsWith("    "))
            {
                ParseCommandOfCurrentLine();
            }
            else
            {
                var executedFunction = _callStack.Pop();
                _currentLineNumber = executedFunction.CallLine + 1;
            }
        }

        private void ParseCommandOfCurrentLine()
        {
            _currentLineParts = _codeLines[_currentLineNumber].TrimStart().Split(' ');
            var commandName = _currentLineParts[0];

            switch (commandName)
            {
                case "def":
                    SkipFunctionDefinition();
                    break;
                case "call":
                    var funcName = _currentLineParts[1];
                    _callStack.Push((funcName, _currentLineNumber));
                    _currentLineNumber = _functionNameToDefinitionLine[funcName];
                    break;
                default:
                    _commandForExecute = CreateCurrentCommandFrom(_currentLineParts);
                    _currentLineNumber++;
                    break;
            }
        }

        private void SkipFunctionDefinition()
        {
            _currentLineNumber++;

            while (_currentLineNumber < _codeLines.Length && _codeLines[_currentLineNumber].StartsWith("    "))
                _currentLineNumber++;
        }

        private Command CreateCurrentCommandFrom(string[] commandParts)
        {
            if (commandParts.Length <= 2)
                return new Command(commandParts[0], commandParts[1]);

            var commandValue = int.Parse(commandParts[2]);
            return new CommandWithValue(commandParts[0], commandParts[1], commandValue);
        }

        public class Command
        {
            public string Name { get; }
            public string VariableName { get; }

            public Command(string name, string variableName)
            {
                Name = name;
                VariableName = variableName;
            }

            // для удобства при дебаге
            public override string ToString() => $"{Name} {VariableName}";
        }

        private class CommandWithValue : Command
        {
            public int Value { get; }

            public CommandWithValue(string name, string variableName, int value)
                : base(name, variableName)
            {
                Value = value;
            }

            public override string ToString() => $"{Name} {VariableName} {Value}";
        }

        private class CommandExecutor
        {
            private readonly TextWriter _writer;

            private readonly Dictionary<string, int> _variableNameToValue = new Dictionary<string, int>();

            public CommandExecutor(TextWriter writer)
            {
                _writer = writer;
            }

            public bool TryExecute(Command command)
            {
                if (command.Name != "set" && !MemoryContainsVariableWithName(command.VariableName))
                    return false;

                if (command is CommandWithValue valueCommand)
                    Execute(valueCommand);
                else
                    Execute(command);

                return true;
            }

            private bool MemoryContainsVariableWithName(string name)
            {
                if (_variableNameToValue.ContainsKey(name)) return true;

                _writer.WriteLine("Переменная отсутствует в памяти");
                return false;
            }

            private void Execute(CommandWithValue valueCommand)
            {
                switch (valueCommand.Name)
                {
                    case "set":
                        _variableNameToValue[valueCommand.VariableName] = valueCommand.Value;
                        break;
                    case "sub":
                        _variableNameToValue[valueCommand.VariableName] -= valueCommand.Value;
                        break;
                }
            }

            private void Execute(Command command)
            {
                switch (command.Name)
                {
                    case "rem":
                        _variableNameToValue.Remove(command.VariableName);
                        break;
                    case "print":
                        _writer.WriteLine(_variableNameToValue[command.VariableName]);
                        break;
                }
            }
        }
    }
}