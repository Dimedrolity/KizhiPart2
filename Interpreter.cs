using System;
using System.Collections.Generic;
using System.IO;

namespace KizhiPart2
{
    public class Interpreter
    {
        private bool _isSourceCodeStarts;

        private string[] _codeLines;
        private int _currentLineNumber;
        private bool IsCurrentLineInsideCode => _currentLineNumber < _codeLines.Length;
        private string CurrentLineOfCode => _codeLines[_currentLineNumber];
        private bool IsCodeEnd => _codeLines != null && !IsCurrentLineInsideCode && _callStack.Count == 0;

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
                _codeLines = command.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

                FindAllFunctionDefinitions();
                _isSourceCodeStarts = false;
            }

            switch (command)
            {
                case "set code":
                    _isSourceCodeStarts = true;
                    break;
                case "run":
                    Run();
                    break;
            }

            if (IsCodeEnd || !_commandExecutor.IsPreviousCommandExecuted)
                Reset();


            void FindAllFunctionDefinitions()
            {
                while (IsCurrentLineInsideCode)
                {
                    if (CurrentLineOfCode.StartsWith("def"))
                    {
                        var currentLineParts = CurrentLineOfCode
                            .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        var functionName = currentLineParts[1];
                        _functionNameToDefinitionLine[functionName] = _currentLineNumber;
                    }

                    _currentLineNumber++;
                }

                _currentLineNumber = 0;
            }

            void Reset()
            {
                _currentLineNumber = 0;
                _commandExecutor.Reset();
            }
        }

        private void Run()
        {
            while (_commandExecutor.IsPreviousCommandExecuted && !IsCodeEnd)
            {
                ParseCurrentCodeLine();

                if (_commandForExecute == null)
                    continue;

                _commandExecutor.Execute(_commandForExecute);
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
            else if (IsCurrentLineInsideCode && CurrentLineOfCode.StartsWith("    "))
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
            var currentLineParts = CurrentLineOfCode.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var commandName = currentLineParts[0];

            switch (commandName)
            {
                case "def":
                    SkipFunctionDefinition();
                    break;
                case "call":
                    PushFunctionToStackAndGoToDefinition();
                    break;
                default:
                    _commandForExecute = CreateCommandFromCurrentLineParts();
                    _currentLineNumber++;
                    break;
            }


            void SkipFunctionDefinition()
            {
                _currentLineNumber++;

                while (IsCurrentLineInsideCode && CurrentLineOfCode.StartsWith("    "))
                    _currentLineNumber++;
            }

            void PushFunctionToStackAndGoToDefinition()
            {
                var functionName = currentLineParts[1];
                _callStack.Push((functionName, _currentLineNumber));
                _currentLineNumber = _functionNameToDefinitionLine[functionName];
            }

            Command CreateCommandFromCurrentLineParts()
            {
                if (currentLineParts.Length <= 2)
                    return new Command(currentLineParts[0], currentLineParts[1]);

                var commandValue = int.Parse(currentLineParts[2]);
                return new CommandWithValue(currentLineParts[0], currentLineParts[1], commandValue);
            }
        }
    }

    internal class Command
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

    internal class CommandWithValue : Command
    {
        public int Value { get; }

        public CommandWithValue(string name, string variableName, int value)
            : base(name, variableName)
        {
            Value = value;
        }

        public override string ToString() => $"{Name} {VariableName} {Value}";
    }

    internal class CommandExecutor
    {
        public bool IsPreviousCommandExecuted { get; private set; } = true;

        private readonly VariablesStorage _variablesStorage = new VariablesStorage();

        private readonly TextWriter _writer;

        public CommandExecutor(TextWriter writer)
        {
            _writer = writer;
        }

        public void Execute(Command command)
        {
            if (command.Name != "set" && !MemoryContainsVariableWithName(command.VariableName))
            {
                IsPreviousCommandExecuted = false;
                return;
            }

            if (command is CommandWithValue commandWithValue)
                ExecuteCommandWithValue(commandWithValue);
            else
                ExecuteCommand(command);
        }

        private bool MemoryContainsVariableWithName(string variableName)
        {
            if (_variablesStorage.ContainsVariableWithName(variableName)) return true;

            _writer.WriteLine("Переменная отсутствует в памяти");
            return false;
        }

        private void ExecuteCommandWithValue(CommandWithValue commandWithValue)
        {
            switch (commandWithValue.Name)
            {
                case "set":
                    _variablesStorage.SetValueOfVariableWithName(commandWithValue.VariableName, commandWithValue.Value);
                    break;
                case "sub":
                    var currentValue = _variablesStorage.GetValueOfVariableWithName(commandWithValue.VariableName);
                    var valueAfterSub = currentValue - commandWithValue.Value;
                    _variablesStorage.SetValueOfVariableWithName(commandWithValue.VariableName, valueAfterSub);
                    break;
            }
        }

        private void ExecuteCommand(Command command)
        {
            switch (command.Name)
            {
                case "rem":
                    _variablesStorage.RemoveVariableWithName(command.VariableName);
                    break;
                case "print":
                    var value = _variablesStorage.GetValueOfVariableWithName(command.VariableName);
                    _writer.WriteLine(value);
                    break;
            }
        }

        public void Reset()
        {
            IsPreviousCommandExecuted = true;
            _variablesStorage.Clear();
        }
    }

    internal class VariablesStorage
    {
        private readonly Dictionary<string, int> _variableNameToValue = new Dictionary<string, int>();

        public bool ContainsVariableWithName(string variableName)
        {
            return _variableNameToValue.ContainsKey(variableName);
        }

        public int GetValueOfVariableWithName(string variableName)
        {
            return _variableNameToValue[variableName];
        }

        public void SetValueOfVariableWithName(string variableName, int value)
        {
            if (value <= 0)
                throw new ArgumentException("Значениями переменных могут быть только натуральные числа");

            _variableNameToValue[variableName] = value;
        }

        public void RemoveVariableWithName(string variableName)
        {
            _variableNameToValue.Remove(variableName);
        }

        public void Clear()
        {
            _variableNameToValue.Clear();
        }
    }
}