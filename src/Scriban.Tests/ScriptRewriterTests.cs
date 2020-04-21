// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Text;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    [TestFixture]
    public class ScriptRewriterTests
    {
        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void ScriptRewriter_Returns_Original_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var rewriter = new TestCloneScriptRewriter();
            var result = rewriter.Visit(template.Page);

            // The base ScriptRewriter never changes any node, so we should end up with the same instance
            Assert.AreNotSame(template.Page, result);
        }

        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void LeafCopyScriptRewriter_Returns_Identical_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var rewriter = new TestCloneScriptRewriter();
            var result = rewriter.Visit(template.Page);

            // This rewriter makes copies of leaf nodes instead of returning the original nodes,
            // so we should end up with another instance identical to the original.
            Assert.AreNotSame(template.Page, result);
            Assert.AreEqual(ToText(template.Page), ToText(result));
        }

        private string ToText(ScriptNode node)
        {
            var output = new StringBuilder();
            var context = new TemplateRewriterContext(new StringBuilderOutput(output));
            context.Write(node);
            return output.ToString();
        }

        private Template LoadTemplate(string inputName)
        {
            var templateSource = TestFilesHelper.LoadTestFile(inputName);
            var parser =
                inputName.Contains("500-liquid")
                    ? (Func<string, string, ParserOptions?, LexerOptions?, Template>) Template.ParseLiquid
                    : Template.Parse;

            var template = parser(templateSource, inputName, default, default);
            if (template.HasErrors || template.Page == null)
                Assert.Ignore("Template didn't parse correctly");

            return template;
        }
        
        private class TestCloneScriptRewriter : ScriptRewriter
        {
        }
    }
}