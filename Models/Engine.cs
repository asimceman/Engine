using Newtonsoft.Json.Linq;
using RazorLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
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
        string FactoryPath = "";


        public Engine()
        {
                GeneratorPath = System.AppDomain.CurrentDomain.BaseDirectory;
                CurrentDirectory = Directory.GetCurrentDirectory();
        }

        public async Task InputProcces(string[] command)
        {

            InputValidation(command);

            Model["name"] = command[2];
            ArrayList inputtedValues = new ArrayList();
            inputtedValues.Add("name");
            var first = "";
            if (command.Length > 3)
            {
                for (var i = 3; i < command.Length; i++)
                {
                    if (i % 2 != 0)
                    {
                        first = command[i].Replace("-", "");
                    }
                    else
                    {
                        Model[first] = command[i];
                        Console.WriteLine("First: " + first);
                        Console.WriteLine("Second: " + command[i]);
                    }
                    inputtedValues.Add(first);
                }
            }


            var schema = GetSchema(command[1]);

            JArray required = schema["required"] as JArray;
            ArrayList requiredList = new ArrayList(required.ToObject<ArrayList>());

            InsertModel(schema, inputtedValues);

            foreach (var question in Prompts)
            {
                if (inputtedValues.Contains(question.Key))
                    continue;
                PromptAction(question, requiredList);
            }

            GetTemplatesAndGenerate(TemplatePath);
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

        public Dictionary<string, Object> GetSchema(string componentToGenerate) {

            if (Model.ContainsKey("templatesDirectory"))
            {
                if(Model["templatesDirectory"].ToString() != "")
                    FactoryPath = CurrentDirectory + "/" + Model["templatesDirectory"] + "/" + componentToGenerate;
                else
                    FactoryPath = CurrentDirectory + "/"  + componentToGenerate;
            }
            else
            {
                if(File.Exists(CurrentDirectory + "/" + componentToGenerate + "/schema.json"))
                    FactoryPath = CurrentDirectory + "/" + componentToGenerate;
                else
                    FactoryPath = GeneratorPath + "/Generators/" + componentToGenerate;
            }
            TemplatePath = FactoryPath + "./Files";

            var schemaPath = Path.Combine(FactoryPath + "/schema.json");
            JObject schema = JObject.Parse(File.ReadAllText(schemaPath)); //getting json for wanted object
            return schema.ToObject<Dictionary<string, Object>>();
        }

        public void InsertModel(dynamic schema, ArrayList inputtedValues)
        {
            if (schema.ContainsKey("properties"))
            {
                var properties = schema["properties"];
                foreach (JProperty property in (JToken)properties)
                {
                    var field = property.Path;
                    if (inputtedValues.Contains(field))
                        continue;
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
            foreach (string TemplateFile in GetFiles(templateDirectory))
            {
                var templateName = TemplateFile.Replace(templateDirectory, "");
                templateName = templateName.Replace(@"\", "");
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
                Generate(TemplateFile, templateName);
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

        public dynamic DetermineVariableType(dynamic enumQuestion, string selected) 
        {
            if (enumQuestion["type"].ToString() == "int")
            {
                return int.Parse(selected);
            }
            return selected;
        }

        public void QuestionArrayPrompt(dynamic question)
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
                            
                            tip.Add(enumQuestion["name"].ToString(), DetermineVariableType(enumQuestion, selected));
                            
                        }
                        else
                        {
                            Console.WriteLine(enumQuestion["question"].ToString());
                            var inputtedValue = Console.ReadLine();
                            tip.Add(enumQuestion["name"].ToString(), DetermineVariableType(enumQuestion, inputtedValue));
                        }
                        Console.WriteLine("");
                    }

                    filters.Add(tip);

                }
            }
            while (filter != "");
            Model[question.Key] = filters;
        }

        

        public void PromptAction(dynamic question, ArrayList required)
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
                    QuestionArrayPrompt(question);
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
                        RequiredProperty(question, inputtedValue, requiredField);
                    else if (inputtedValue != "")
                        Model[question.Key] = inputtedValue;
                }
                Console.WriteLine("");
            }
        }

        public void RequiredProperty(dynamic question, string inputtedValue, string requiredField) 
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
            var destination = System.IO.Path.Combine(destinationPath, outputFileName);

            using (StreamWriter outputFile = new StreamWriter(destination))
            {
                await outputFile.WriteAsync(result);
                Console.WriteLine("Successfully generated " + outputFileName);
            }
        }
    }
}







