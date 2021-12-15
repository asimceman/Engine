using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchematicsProject
{

    public class Engine
    {
        private IDictionary<string, Object> Model = new  ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> Prompts = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> Enums = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> Types = new ExpandoObject() as IDictionary<string, Object>;
        string GeneratorPath = "";
        string CurrentDirectory = "";
        string TemplatePath = "";


        public Engine()
        {
                GeneratorPath = System.AppDomain.CurrentDomain.BaseDirectory;
                CurrentDirectory = Directory.GetCurrentDirectory();
        }

        public async Task InputProcces(string Input)
        {
            Input = Regex.Replace(Input, @"\s+", " ");   
            var Command = Input.Split(" ");

            InputValidation(Command);

            var Schema = getSchema(Command[1]);

            JArray Required = Schema["required"] as JArray;
            ArrayList RequiredList = new ArrayList(Required.ToObject<ArrayList>());

            InsertModel(Schema);
            
            Model["name"] = Command[2];
            ArrayList InputtedValues = new ArrayList();
            InputtedValues.Add("name");
            if (Command.Length > 3)
            {
                for (var i = 3; i < Command.Length; i++)
                {
                    var CommandSplit = Command[i].Split("-");
                    Model[CommandSplit[0]] = CommandSplit[1];
                    InputtedValues.Add(CommandSplit[0]);
                }
            }

            




            foreach (var Question in Prompts)
            {
                if (InputtedValues.Contains(Question.Key))
                    continue;
                promptAction(Question, RequiredList);
            }

            var TemplateDirectory = GeneratorPath + TemplatePath + "/";

            if (Model.ContainsKey("templatesDirectory"))
            {
                TemplateDirectory = Model["templatesDirectory"].ToString();
                if (!TemplateDirectory.EndsWith("/"))
                {
                    TemplateDirectory += "/";
                }
            }

            GetTemplatesAndGenerate(TemplateDirectory);
        }

        public void InputValidation(dynamic Command)
        {
            if (Command.Length < 3)
            {
                throw new ArgumentException("Invalid input");
            }

            if (Command[0] != "-t")
                throw new ArgumentException("invalid input");
        }

        public Dictionary<string, Object> getSchema(string ComponentToGenerate) {
            string CollectionPath = Path.Combine(GeneratorPath, "collection.json");

            JObject Data = JObject.Parse(File.ReadAllText(CollectionPath));
            var Schematics = Data["schematics"];
            var Component = Schematics[ComponentToGenerate];

            if (Component == null)
            {
                throw new Exception("Schematics doesn't exist");
            }
            var FactoryPath = Schematics[ComponentToGenerate]["factory"];
            TemplatePath = FactoryPath + "./Files";

            var SchemaPath = Path.Combine(GeneratorPath, Component["schema"].ToString());
            JObject Schema = JObject.Parse(File.ReadAllText(SchemaPath)); //getting json for wanted object
            return Schema.ToObject<Dictionary<string, Object>>();
        }

        public void InsertModel(dynamic Schema)
        {
            if (Schema.ContainsKey("properties"))
            {
                var Properties = Schema["properties"];
                foreach (JProperty Property in (JToken)Properties)
                {
                    var Field = Property.Path;
                    if (Property.Value["type"].ToString() == "boolean")
                    {
                        bool BoolValue = false;
                        if (Property.Value["default"].ToString() == "True")
                            BoolValue = true;
                        Model.Add(Field, BoolValue);
                    }
                    else
                    {
                        Model.Add(Field, Property.Value["default"]); //getting default value for every key
                    }
                    Prompts.Add(Field, Property.Value["x-prompt"]);
                    Enums.Add(Field, Property.Value["enum"]);
                    Types.Add(Field, Property.Value["type"]);
                }
            }
        }

        public void GetTemplatesAndGenerate(string TemplateDirectory)
        {
            foreach (string TemplateFile in GetFiles(TemplateDirectory))
            {
                var TemplateName = TemplateFile.Replace(TemplateDirectory, "");
                TemplateName = TemplateName.Replace(@"\", "/");
                //TemplateName = TemplateName.Replace(CurrentPath + TemplatePath + "/", "");
                string Pattern = @"__(.*?)__";
                Regex Regex = new Regex(Pattern);
                foreach (Match match in Regex.Matches(TemplateName, Pattern))
                {
                    TemplateName = Regex.Replace(TemplateName, Model[match.Groups[1].Value].ToString(), 1);
                }

                int Index = TemplateName.LastIndexOf(".");
                if (Index >= 0)
                    TemplateName = TemplateName.Substring(0, Index);
                Generate(TemplateFile, TemplateName);
            }
        }

        public IEnumerable<string> GetFiles(string Path)
        {
            var InitialPath = Path;
            Queue<string> Queue = new Queue<string>();
            Queue.Enqueue(Path);
            while (Queue.Count > 0)
            {
                Path = Queue.Dequeue();
                try
                {
                    foreach (string SubDir in Directory.GetDirectories(Path))
                    {
                        var DirectoryInsideTemplates = SubDir.Replace(InitialPath, "");
                        var PlaceToCreateDirectory = CurrentDirectory + "/" + DirectoryInsideTemplates;
                        System.IO.Directory.CreateDirectory(PlaceToCreateDirectory);

                        Queue.Enqueue(SubDir + "/");
                    }
                }
                catch (Exception Ex)
                {
                    Console.Error.WriteLine(Ex);
                }
                string[] Files = null;
                try
                {
                    Files = Directory.GetFiles(Path);
                }
                catch (Exception Ex)
                {
                    Console.Error.WriteLine(Ex);
                }
                if (Files != null)
                {
                    for (int i = 0; i < Files.Length; i++)
                    {
                        yield return Files[i];
                    }
                }
            }
        }

        public dynamic determineVariableType(dynamic EnumQuestion, string selected) 
        {
            if (EnumQuestion["type"].ToString() == "int")
            {
                return int.Parse(selected);
            }
            return selected;
        }

        public void questionArrayPrompt(dynamic Question)
        {
            var First = true;
            ArrayList Filters = new ArrayList();
            var Filter = "";
            var i = 0;
            JArray QuestionHelper = Enums[(Question.Key).ToString()] as JArray;
            ArrayList Questions = new ArrayList(QuestionHelper.ToObject<ArrayList>());
            do
            {
                if (!First)
                {
                    Console.WriteLine(Question.Value);
                }
                else
                {
                    First = false;
                }

                Filter = Console.ReadLine();
                Console.WriteLine("");
                if (!string.IsNullOrEmpty(Filter))
                {
                    IDictionary<string, dynamic> Tip = new ExpandoObject() as IDictionary<string, dynamic>;
                    Tip.TryAdd("name", Filter);
                    Console.CursorVisible = false;
                    foreach (JObject EnumQuestion in Questions)
                    {
                        if (EnumQuestion["options"] != null)
                        {
                            string[] options = EnumQuestion["options"].ToObject<string[]>();
                            Console.WriteLine(EnumQuestion["question"].ToString());
                            Menu Menu = new Menu(options);
                            int SelectedIndex = Menu.Run();
                            var selected = options[SelectedIndex];
                            
                            Tip.Add(EnumQuestion["name"].ToString(), determineVariableType(EnumQuestion, selected));
                            
                        }
                        else
                        {
                            Console.WriteLine(EnumQuestion["question"].ToString());
                            var inputtedValue = Console.ReadLine();
                            Tip.Add(EnumQuestion["name"].ToString(), determineVariableType(EnumQuestion, inputtedValue));
                        }
                        Console.WriteLine("");
                    }

                    Filters.Add(Tip);

                }
            }
            while (Filter != "");
            Model[Question.Key] = Filters;
        }

        

        public void promptAction(dynamic Question, ArrayList Required)
        {
            if (Question.Value != null)
            {
                Console.CursorVisible = true;
                string RequiredField = "";
                if (Required.Contains(Question.Key)) {
                    RequiredField = " (This field is required, if you don't provide a value, it will have the same value as the provided name)";
                }
                Console.WriteLine(Question.Value + RequiredField);

                if (Types[Question.Key].ToString() == "questionArray") {
                    questionArrayPrompt(Question);
                }
                else if (Enums[Question.Key] != null)
                {
                    Console.CursorVisible = false;
                    IEnumerable EnumerableQuestions = Enums[Question.Key] as IEnumerable;
                    List<string> Options = new List<string>();
                    foreach (var Option in EnumerableQuestions)
                    {
                        Options.Add(Option.ToString());
                    }
                    Menu Menu = new Menu(Options.ToArray());
                    int SelectedIndex = Menu.Run();
                    Model[Question.Key] = Options[SelectedIndex];
                }

                else if (Types[Question.Key].ToString() == "boolean")
                {
                    var Def = Console.ReadLine();
                    if (Def != "")
                    {
                        var res = Model[Question.Key];
                        if (Def == "No" || Def == "no" || Def == "n" || Def == "NO")
                            res = false;
                        else if (Def == "Yes" || Def == "yes" || Def == "y" || Def == "YES")
                            res = true;
                        Model[Question.Key] = res;
                    }
                }
                else
                {
                    var InputtedValue = Console.ReadLine();
                    if (Required.Contains(Question.Key) && InputtedValue == "")
                        Model[Question.Key] = Model["name"];
                    if (InputtedValue != "")
                        Model[Question.Key] = InputtedValue;
                }
                Console.WriteLine("");
            }
        }

        public async void Generate(string TemplateFile, string OutputFileName)
        {
            string DestinationPath = CurrentDirectory + "/";
            var Engine = new RazorLightEngineBuilder()
                  // required to have a default RazorLightProject type, but not required to create a template from string.
                  .UseEmbeddedResourcesProject(typeof(Engine))
                  .UseMemoryCachingProvider()
                  .Build();

            string Template = "";
            try
            {
                Template = File.ReadAllText(TemplateFile);
            }
            catch (Exception e)
            {
                throw e;
            }

            string Result = await Engine.CompileRenderStringAsync("templateKey2", Template, Model);
            
            using (StreamWriter OutputFile = new StreamWriter(System.IO.Path.Combine(DestinationPath, OutputFileName)))
            {
                await OutputFile.WriteAsync(Result);
                Console.WriteLine("Successfully generated " + OutputFileName);
            }
        }
    }
}







