using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorLight;
using SchematicsProject.component_list;
using SchematicsProject.IndexClasses;
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
        private IDictionary<string, Object> model = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> prompts = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> enums = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> types = new ExpandoObject() as IDictionary<string, Object>;
        string CurrentPath = "";
        string CurrentDirectory = "";

        public IConfiguration Configuration { get; set; }

        public Engine(IConfiguration configuration)
        {
            this.Configuration = configuration;
            if (Configuration.GetSection("Env").Value == "Publish")
            {
                
                CurrentPath = System.AppDomain.CurrentDomain.BaseDirectory;
                CurrentDirectory = Directory.GetCurrentDirectory();
            }
            else if (Configuration.GetSection("Env").Value == "Debug")
            {
                var enviroment = System.Environment.CurrentDirectory;
                CurrentPath = Directory.GetParent(enviroment).Parent.FullName.Replace("\\bin", "");
                CurrentDirectory = CurrentPath;
            }
        }



        public async Task Input(string input)
        {
            string a = Configuration.GetSection("Name").Value;
            Console.WriteLine(a);

            input = Regex.Replace(input, @"\s+", " ");   

            var command = input.Split(" ");
            if (command.Length < 3)
            {
                throw new ArgumentException("Invalid input");
            }

            if (command[0] != "-t")
                throw new ArgumentException("invalid input"); 

            string path = Path.Combine(CurrentPath, "collection.json");


            JObject data = JObject.Parse(File.ReadAllText(path));
            var schematics = data["schematics"];
            var component = schematics[command[1]];

            if (component == null)
            {
                throw new Exception("Schematics doesn't exist");
            }
            var factoryPath = schematics[command[1]]["factory"];
            var templatePath = factoryPath  + "./Files";
            JArray required = null;
            ArrayList required2 = new ArrayList();

            path = Path.Combine(CurrentPath, component["schema"].ToString());
            JObject schema = JObject.Parse(File.ReadAllText(path)); //getting json for wanted object
            var dictionary = schema.ToObject<Dictionary<string, Object>>();
            required = dictionary["required"] as JArray;
            required2 = required.ToObject<ArrayList>();
            //var templates = dictionary["templates"];
            if (dictionary.ContainsKey("properties"))
            {
                var properties = dictionary["properties"];
                foreach (JProperty property in (JToken)properties)
                {
                    var field = property.Path;
                    if (property.Value["type"].ToString() == "boolean")
                    {
                        bool def = false;
                        if (property.Value["default"].ToString() == "True")
                            def = true;
                        model.Add(field, def);
                    }
                    else
                    {
                        model.Add(field, property.Value["default"]); //getting default value for every key
                    }
                    prompts.Add(field, property.Value["x-prompt"]);
                    enums.Add(field, property.Value["enum"]);
                    types.Add(field, property.Value["type"]);
                }
            }
            
            model["name"] = command[2];
            ArrayList inputedValues = new ArrayList();
            inputedValues.Add("name");
            if (command.Length > 3)
            {
                for (var i = 3; i < command.Length; i++)
                {
                    var command2 = command[i].Split(":");
                    var firstPart = command2[0];
                    var secondPart = command2[1];
                    model[firstPart] = secondPart;
                    inputedValues.Add(firstPart);

                }
            }



            foreach (var question in prompts)
            {
                if (inputedValues.Contains(question.Key))
                    continue;
                model = promptAction(question, required2);
            }

            ClassHelper(command[1]);

            var dirFile = CurrentPath + templatePath + "/";
            

            foreach (string fileName in GetFiles(dirFile))
            {
                var templateName = fileName.Replace(CurrentPath + templatePath + "/", "");
                templateName = templateName.Replace(@"\", "/");
                
                //templateName = templateName.Split('/').Last();
                templateName = templateName.Replace(CurrentPath + templatePath + "/", "");
                string pattern = @"__(.*?)__";
                Regex regex = new Regex(pattern);
                foreach (Match match in Regex.Matches(templateName, pattern))
                {
                    templateName = regex.Replace(templateName, model[match.Groups[1].Value].ToString(), 1);
                }

                int index = templateName.LastIndexOf(".");
                if (index >= 0)
                    templateName = templateName.Substring(0, index);
                Generate(fileName, templateName);
            }
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var firstPath = path;
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                //Console.WriteLine("Path " + path);
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        
                        var directoryInsideTemplates = subDir.Replace(firstPath, "");
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

        public void ClassHelper(string command)
        {
            if (command == "component")
            {
                model = Component.AddDynamic(model);
            }
            else if (command == "component-list")
            {
                model = ComponentList.AddDynamic(model);
            }
        }

        public dynamic promptAction(dynamic question, dynamic required)
        {
            if (question.Value != null)
            {
                Console.CursorVisible = true;
                Console.WriteLine(question.Value);
                if (enums[question.Key] != null)
                {
                    Console.CursorVisible = false;
                    IEnumerable enumerable = enums[question.Key] as IEnumerable;
                    List<string> opcije = new List<string>();
                    foreach (var option in enumerable)
                    {
                        opcije.Add(option.ToString());
                    }


                    Menu menu = new Menu(opcije.ToArray());
                    int selectedIndex = menu.Run();
                    model[question.Key] = opcije[selectedIndex];

                }

                else if (types[question.Key].ToString() == "boolean")
                {
                    var def = Console.ReadLine();
                    if (def != "")
                    {
                        var res = model[question.Key];
                        if (def == "No" || def == "no" || def == "n" || def == "NO")
                            res = false;
                        else if (def == "Yes" || def == "yes" || def == "y" || def == "YES")
                            res = true;
                        model[question.Key] = res;
                    }
                }
                else
                {
                    var inputedValue = Console.ReadLine();
                    if (required.Contains(question.Key) && inputedValue == "")
                        model[question.Key] = model["name"];
                    if (inputedValue != "")
                        model[question.Key] = inputedValue;
                }
                Console.WriteLine("");
            }
            return model;
        }

        public async void Generate(string templatePath, string outputFileName)
        {
            string path = CurrentDirectory + "/";
            var engine = new RazorLightEngineBuilder()
                  // required to have a default RazorLightProject type, but not required to create a template from string.
                  .UseEmbeddedResourcesProject(typeof(Component))
                  .UseMemoryCachingProvider()
                  .Build();

            string template = "";
            try
            {
                template = File.ReadAllText(templatePath);
            }
            catch (Exception e)
            {
                throw e;
            }

            string result = await engine.CompileRenderStringAsync("templateKey2", template, model);
            

            //string outputName = model["name"].ToString() + "." + language;

            

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, outputFileName)))
            {
                await outputFile.WriteAsync(result);
                Console.WriteLine("Successfully generated " + outputFileName);
            }
        }
    }
}







