using Newtonsoft.Json.Linq;
using RazorLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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

        public async Task InputProcces(string input)
        {
            input = Regex.Replace(input, @"\s+", " ");   
            var command = input.Split(" ");

            InputValidation(command);

            var schema = getSchema(command[1]);

            JArray required = schema["required"] as JArray;
            ArrayList requiredList = new ArrayList(required.ToObject<ArrayList>());

            InsertModel(schema);
            
            Model["name"] = command[2];
            ArrayList inputtedValues = new ArrayList();
            inputtedValues.Add("name");
            if (command.Length > 3)
            {
                for (var i = 3; i < command.Length; i++)
                {
                    var commandSplit = command[i].Split("-");
                    Model[commandSplit[0]] = commandSplit[1];
                    inputtedValues.Add(commandSplit[0]);
                }
            }

            




            foreach (var question in Prompts)
            {
                if (inputtedValues.Contains(question.Key))
                    continue;
                promptAction(question, requiredList);
            }

            var templateDirectory = GeneratorPath + TemplatePath + "/";

            if (Model.ContainsKey("templatesDirectory"))
            {
                templateDirectory = Model["templatesDirectory"].ToString();
                if (!templateDirectory.EndsWith("/"))
                {
                    templateDirectory += "/";
                }
            }

            GetTemplatesAndGenerate(templateDirectory);
        }

        public void InputValidation(dynamic command)
        {
            if (command.Length < 3)
            {
                throw new ArgumentException("Invalid input");
            }

            if (command[0] != "-t")
                throw new ArgumentException("invalid input");
        }

        public Dictionary<string, Object> getSchema(string componentToGenerate) {
            string collectionPath = Path.Combine(GeneratorPath, "collection.json");

            JObject data = JObject.Parse(File.ReadAllText(collectionPath));
            var schematics = data["schematics"];
            var component = schematics[componentToGenerate];

            if (component == null)
            {
                throw new Exception("Schematics doesn't exist");
            }
            var factoryPath = schematics[componentToGenerate]["factory"];
            TemplatePath = factoryPath + "./Files";

            var schemaPath = Path.Combine(GeneratorPath, component["schema"].ToString());
            JObject schema = JObject.Parse(File.ReadAllText(schemaPath)); //getting json for wanted object
            return schema.ToObject<Dictionary<string, Object>>();
        }

        public void InsertModel(dynamic schema)
        {
            if (schema.ContainsKey("properties"))
            {
                var properties = schema["properties"];
                foreach (JProperty property in (JToken)properties)
                {
                    var field = property.Path;
                    if (property.Value["type"].ToString() == "boolean")
                    {
                        bool boolValue = false;
                        if (property.Value["default"].ToString() == "True")
                            boolValue = true;
                        Model.Add(field, boolValue);
                    }
                    else
                    {
                        Model.Add(field, property.Value["default"]); //getting default value for every key
                    }
                    Prompts.Add(field, property.Value["x-prompt"]);
                    Enums.Add(field, property.Value["enum"]);
                    Types.Add(field, property.Value["type"]);
                }
            }
        }

        public void GetTemplatesAndGenerate(string templateDirectory)
        {
            foreach (string templateFile in GetFiles(templateDirectory))
            {
                var templateName = templateFile.Replace(templateDirectory, "");
                templateName = templateName.Replace(@"\", "/");
                //TemplateName = TemplateName.Replace(CurrentPath + TemplatePath + "/", "");
                string pattern = @"__(.*?)__";
                Regex regex = new Regex(pattern);
                foreach (Match match in Regex.Matches(templateName, pattern))
                {
                    templateName = regex.Replace(templateName, Model[match.Groups[1].Value].ToString(), 1);
                }

                int index = templateName.LastIndexOf(".");
                if (index >= 0)
                    templateName = templateName.Substring(0, index);
                Generate(templateFile, templateName);
            }
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var initialPath = path;
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        var directoryInsideTemplates = subDir.Replace(initialPath, "");
                        var placeToCreateDirectory = CurrentDirectory + "/" + directoryInsideTemplates;
                        System.IO.Directory.CreateDirectory(placeToCreateDirectory);

                        queue.Enqueue(subDir + "/");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }

        public dynamic determineVariableType(dynamic enumQuestion, string selected) 
        {
            if (enumQuestion["type"].ToString() == "int")
            {
                return int.Parse(selected);
            }
            return selected;
        }

        public void questionArrayPrompt(dynamic question)
        {
            var first = true;
            ArrayList filters = new ArrayList();
            var filter = "";
            JArray questionHelper = Enums[(question.Key).ToString()] as JArray;
            ArrayList questions = new ArrayList(questionHelper.ToObject<ArrayList>());
            do
            {
                if (!first)
                {
                    Console.WriteLine(question.Value);
                }
                else
                {
                    first = false;
                }

                filter = Console.ReadLine();
                if (!string.IsNullOrEmpty(filter))
                {
                    IDictionary<string, dynamic> tip = new ExpandoObject() as IDictionary<string, dynamic>;
                    tip.TryAdd("name", filter);
                    Console.CursorVisible = false;
                    foreach (JObject enumQuestion in questions)
                    {
                        if (enumQuestion["options"] != null)
                        {
                            string[] options = enumQuestion["options"].ToObject<string[]>();
                            Console.WriteLine(enumQuestion["question"].ToString());
                            Menu menu = new Menu(options);
                            int selectedIndex = menu.Run();
                            var selected = options[selectedIndex];
                            
                            tip.Add(enumQuestion["name"].ToString(), determineVariableType(enumQuestion, selected));
                            
                        }
                        else
                        {
                            Console.WriteLine(enumQuestion["question"].ToString());
                            var inputtedValue = Console.ReadLine();
                            tip.Add(enumQuestion["name"].ToString(), determineVariableType(enumQuestion, inputtedValue));
                        }
                        Console.WriteLine("");
                    }

                    filters.Add(tip);

                }
            }
            while (filter != "");
            Model[question.Key] = filters;
        }

        

        public void promptAction(dynamic question, ArrayList required)
        {
            if (question.Value != null)
            {
                Console.CursorVisible = true;
                string requiredField = "";
                if (required.Contains(question.Key)) {
                    requiredField = " (This field is required)";
                }
                Console.WriteLine(question.Value + requiredField);

                if (Types[question.Key].ToString() == "questionArray") {
                    questionArrayPrompt(question);
                }
                else if (Enums[question.Key] != null)
                {
                    Console.CursorVisible = false;
                    IEnumerable enumerableQuestions = Enums[question.Key] as IEnumerable;
                    List<string> options = new List<string>();
                    foreach (var option in enumerableQuestions)
                    {
                        options.Add(option.ToString());
                    }
                    Menu menu = new Menu(options.ToArray());
                    int selectedIndex = menu.Run();
                    Model[question.Key] = options[selectedIndex];
                }

                else if (Types[question.Key].ToString() == "boolean")
                {
                    var boolInputValue = Console.ReadLine();
                    if (boolInputValue != "")
                    {
                        var res = Model[question.Key];
                        if (boolInputValue == "No" || boolInputValue == "no" || boolInputValue == "n" || boolInputValue == "NO")
                            res = false;
                        else if (boolInputValue == "Yes" || boolInputValue == "yes" || boolInputValue == "y" || boolInputValue == "YES")
                            res = true;
                        Model[question.Key] = res;
                    }
                }
                else
                {
                    var inputtedValue = Console.ReadLine();
                    if (required.Contains(question.Key))
                        requiredProperty(question, inputtedValue, requiredField);
                    else if (inputtedValue != "")
                        Model[question.Key] = inputtedValue;
                }
                Console.WriteLine("");
            }
        }

        public void requiredProperty(dynamic question, string inputtedValue, string requiredField) 
        {
            while (inputtedValue == "")
            {
                Console.WriteLine(question.Value + requiredField);
                inputtedValue = Console.ReadLine();
            }
            
            Model[question.Key] = inputtedValue;

        }

        public async void Generate(string templateFile, string outputFileName)
        {
            string destinationPath = CurrentDirectory + "/";
            var engine = new RazorLightEngineBuilder()
                  // required to have a default RazorLightProject type, but not required to create a template from string.
                  .UseEmbeddedResourcesProject(typeof(Engine))
                  .UseMemoryCachingProvider()
                  .Build();

            string template = "";
            try
            {
                template = File.ReadAllText(templateFile);
            }
            catch (Exception e)
            {
                throw e;
            }

            string result = await engine.CompileRenderStringAsync("templateKey2", template, Model);
            
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(destinationPath, outputFileName)))
            {
                await outputFile.WriteAsync(result);
                Console.WriteLine("Successfully generated " + outputFileName);
            }
        }
    }
}







