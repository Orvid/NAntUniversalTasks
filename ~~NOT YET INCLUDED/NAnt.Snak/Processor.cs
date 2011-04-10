using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Snak.Common.Utilities;

namespace Snak.ConfigurationTransformation
{
    /// <summary>
    /// ConfigurationTransformer provides a mechanism to transform an xml configuration file based on certain conditional statements which appear within the file.
    /// 
    /// This is useful in situations where you have to maintain multiple environments each having some different configuration settings. Its allows for all of the 
    /// setting to be stored in one file. For sensitive settings either encrypt the section or store it in the registry. For more information on this see 
    /// http://aspnet.4guysfromrolla.com/articles/021506-1.aspx
    /// </summary>
    /// <example>
    /// The snippet below shows some xml that may appear in an application configuration file. The value within the $( ) portionis is the environment for which 
    /// the setting is applicable. 
    /// <![CDATA[
    ///     <!--  
    ///     #ENV:$(buildVerification) 
    ///       <element_buildVerification mode="Off" />
    ///     #ENV:$(systemTest)
    ///       <element_SystemTest mode="Off" /> 
    ///     #ENV:$(production)
    ///         <element_production mode="Off" />
    ///     #ENV:$(support)
    ///         <element_support mode="Off" />
    ///     #ENV:$(training)
    ///         <element_training mode="Off" />
    ///     #ENV:$(dev)
    ///     -->
    ///         <!-- this is a comment -->
    ///         <element_dev mode="Off" />
    ///     <!-- 
    ///     #END
    ///     -->
    /// ]]>
    /// For example if you ran the above xml through ConfigurationTransformer.Transform specifying the value "buildVerification" 
    /// for the environment variable, the output would be:
    /// <![CDATA[<element_buildVerification mode="Off" />]]>
    /// If you passed "training" the output would be:
    /// <![CDATA[<element_training mode="Off" />]]>
    /// 
    /// The last environment setting “dev” appears out of the XML comments and as such is the value used when the configuration file has not been run through the 
    /// ConfigurationTransformer. This last section is meant to hold the development settings. As part of your build process you transform the dev config file to 
    /// create a new file for the target environments.
    /// </example>
    public class ConfigurationTransformer
    {
        /// <summary>
        /// Transforms a configuration file and saves off the new file to the value specified in 'outFile'.
        /// </summary>
        /// <param name="environment">the target environment</param>
        /// <param name="inFile">the file to transform, e.g. c:\dev\MyApp.config </param>
        /// <param name="outFile">the transformed file will be written to this path, e.g. c:\prod\MyApp.config </param>
        public void Transform(string environment, string fileToProcess, string processedFile)
        {
            if (String.IsNullOrEmpty(fileToProcess))
                throw new ArgumentException("Argument cannot be null or empty", "inFile");

            if (String.IsNullOrEmpty(environment))
                throw new ArgumentException("Argument cannot be null or empty", "environment");

            if (!new FileInfo(fileToProcess).Exists)
                throw new InvalidOperationException("The file to process does not exist at location '" + fileToProcess + "'");

            string fileToProcessContents = File.ReadAllText(fileToProcess);
            string processFileContents = fileToProcessContents;

            IfElseEndIFConstruct ifElseConstruct = null;
            ArrayList ifElseConstructs = new  ArrayList();

            // the ifElseEndIfBlockPattern pattern below will grab then each indivudual markup block in the file 
            string ifElseEndIfBlockPattern = @"(?<ifElseEndIfBlock><!--(\r\n|\s)*?#ENV:[\s]*?\$\([a-z,]*?\)(.|\r\n|\s)*?#END(\r\n|\s)*?-->)";

            // the ifElseStatements pattern below will capture each if/elseif/else block. branchVariables will contain the text 
            // that identifies the branch (i.e. the target environment) and branchInnerText contains the actual text of the branch that should 
            // exist for that environment
            string ifElseStatements = @"(#ENV:)([\s]*?\$\((?<branchVariables>[a-z,]*?)\)){1}((\r\n|\s)*?-->)?(?<branchInnerText>(.|\r\n|\s)*?)(?=(#ENV:|<!--(\r\n|\s)*?#ENV|<!--(\r\n|\s)*?#END|#END(\r\n|\s)*?-->))";

            for (Match ifElseEndIfBlockMatch = Regex.Match(fileToProcessContents, ifElseEndIfBlockPattern, RegexOptions.IgnoreCase); ifElseEndIfBlockMatch.Success; ifElseEndIfBlockMatch = ifElseEndIfBlockMatch.NextMatch())
            {
                ifElseConstruct = new IfElseEndIFConstruct(ifElseEndIfBlockMatch.Value);
                ifElseConstructs.Add(ifElseConstruct);

                string ifElseEndIfBlock = ifElseEndIfBlockMatch.Groups["ifElseEndIfBlock"].Value.ToString();

                for (Match innerMatch = Regex.Match(ifElseEndIfBlock, ifElseStatements, RegexOptions.IgnoreCase); innerMatch.Success; innerMatch = innerMatch.NextMatch())
                {
                    string matchValue = innerMatch.Value;
                    string branchVariables = innerMatch.Groups["branchVariables"].Value;
                    string branchInnerText = innerMatch.Groups["branchInnerText"].Value;

                    string[] branchVariable = branchVariables.Split(',');

                    for (int i = 0; i < branchVariable.Length; i++)
                    {
                        ifElseConstruct.AddIfBranchPart(branchVariable[i], branchInnerText);
                    }
                }

                string replacementText =ifElseConstruct.IfElseConstructInnerText;
                string replacementValue = String.Empty;
                
                if (ifElseConstruct.IfElseBranchPartsByVariable.ContainsKey(environment))
                {
                    replacementValue = ifElseConstruct.IfElseBranchPartsByVariable[environment];

                    // replace any comments with XML comments
                    replacementValue = replacementValue.Replace("/*", "<!--").Replace("*/", "-->");
                }
                else if (ifElseConstruct.IfElseBranchPartsByVariable.ContainsKey("default"))
                {
                    replacementValue = ifElseConstruct.IfElseBranchPartsByVariable["default"];

                    // replace any comments with XML comments
                    replacementValue = replacementValue.Replace("/*", "<!--").Replace("*/", "-->");
                }

                processFileContents = processFileContents.Replace(ifElseConstruct.IfElseConstructInnerText, replacementValue);

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine(ifElseConstruct.ToString());
                }
            }

            if (processedFile == String.Empty)
                processedFile = Path.GetFileNameWithoutExtension(fileToProcess) + "-" + environment + Path.GetExtension(fileToProcess);

            using (new WritableFileScope(new FileInfo(processedFile)))
                File.WriteAllText(processedFile, processFileContents);    
        }
    }
}
