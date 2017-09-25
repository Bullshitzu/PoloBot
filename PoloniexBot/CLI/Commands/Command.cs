using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.CLI.Commands {
    class Command {

        public delegate void ExecuteMethod (string[] parameters);

        private string keyword = "[UNDEFINED]";
        private string description = "[UNDEFINED]";
        private Parameter[] parameters;
        private bool echoCommand;
        ExecuteMethod method;

        public Command () {
            parameters = new Parameter[0];
        }
        public Command (string keyword, string description, ExecuteMethod method) {
            this.keyword = keyword;
            this.description = description;
            this.method = method;
            parameters = new Parameter[0];
            echoCommand = false;
        }
        public Command (string keyword, string description, bool echoCommand, ExecuteMethod method) {
            this.keyword = keyword;
            this.description = description;
            this.method = method;
            this.echoCommand = echoCommand; 
            parameters = new Parameter[0];
        }
        public Command (string keyword, string description, Parameter[] parameters, ExecuteMethod method) {
            this.keyword = keyword;
            this.description = description;
            this.echoCommand = false;
            this.parameters = parameters;
            this.method = method;
        }
        public Command (string keyword, string description, bool echoCommand, Parameter[] parameters, ExecuteMethod method) {
            this.keyword = keyword;
            this.description = description;
            this.echoCommand = echoCommand;
            this.parameters = parameters;
            this.method = method;
        }
        

        public override string ToString () {
            string line = keyword;
            for (int i = 0; i < parameters.Length; i++) {
                line += " " + parameters[i];
            }
            line = line.ToUpper();
            line += " - " + description;
            return line;
        }

        public bool CompareKeyword (string word) {
            return word.ToLower().Trim() == keyword.ToLower().Trim();
        }
        public bool GetEcho () {
            return echoCommand;
        }
        public void Execute (string[] parameters) {
            if (parameters.Length - 1 != this.parameters.Length) throw new Exception("Incorrect syntax. See \"help\" for command formats.");
            method(parameters);
        }
    }

    class Parameter {

        public string exampleWord = "?";

        public Parameter (string exampleWord) {
            this.exampleWord = exampleWord;
        }

        public override string ToString () {
            return "[" + exampleWord + "]";
        }
    }
}
