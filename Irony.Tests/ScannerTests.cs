﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Irony.CompilerServices;
using Irony.Scripting.Ast;

namespace Irony.Tests {

  public class ScanTestGrammar : Grammar {
    public ScanTestGrammar() {
      var comment = new CommentTerminal("comment", "/*", "*/");
      base.NonGrammarTerminals.Add(comment);
      var str = new StringLiteral("str", "'", StringFlags.AllowsLineBreak);
      var stmt = new NonTerminal("stmt");
      stmt.Rule = str | Empty;
      this.Root = stmt; 
    }
  }//class

  [TestClass]
  public class ScannerTests {
    Grammar _grammar; 
    Scanner _scanner; 
    CompilerContext _context; 
    int _state; 

    private void Init(Grammar grammar) {
      _grammar = grammar; 
      var cmp = new Compiler(grammar);
      _scanner = cmp.Scanner;
      _context = new CompilerContext(cmp);
      _context.Mode = CompileMode.VsLineScan;
      _scanner.Prepare(_context, null); 
    }

    private void SetSource(string text) {
      _scanner.VsSetSource(text, 0); 
    }
    private Token Read() {
      Token token = _scanner.VsReadToken(ref _state);
      return token; 
    }

    [TestMethod]
    public void TestVsScanningComment() {
      Init(new ScanTestGrammar());
      SetSource(" /*  ");
      Token token = Read();
      Assert.IsTrue(token.IsSet(AstNodeFlags.IsIncomplete), "Expected incomplete token (line 1)");
      token = Read();
      Assert.IsNull(token, "NULL expected");
      SetSource(" comment ");
      token = Read();
      Assert.IsTrue(token.IsSet(AstNodeFlags.IsIncomplete), "Expected incomplete token (line 2)");
      token = Read();
      Assert.IsNull(token, "NULL expected");
      SetSource(" */ /*x*/");
      token = Read();
      Assert.IsFalse(token.IsSet(AstNodeFlags.IsIncomplete), "Expected complete token (line 3)");
      token = Read();
      Assert.IsFalse(token.IsSet(AstNodeFlags.IsIncomplete), "Expected complete token (line 3)");
      token = Read();
      Assert.IsNull(token, "Null expected.");
    }

    [TestMethod]
    public void TestVsScanningString() {
      Init(new ScanTestGrammar());
      SetSource(" 'abc");
      Token token = Read();
      Assert.IsTrue(token.ValueString == "abc", "Expected incomplete token 'abc' (line 1)");
      Assert.IsTrue(token.IsSet(AstNodeFlags.IsIncomplete), "Expected incomplete token (line 1)");
      token = Read();
      Assert.IsNull(token, "NULL expected");
      SetSource(" def ");
      token = Read();
      Assert.IsTrue(token.ValueString == " def ", "Expected incomplete token ' def ' (line 2)");
      Assert.IsTrue(token.IsSet(AstNodeFlags.IsIncomplete), "Expected incomplete token (line 2)");
      token = Read();
      Assert.IsNull(token, "NULL expected");
      SetSource("ghi' 'x'");
      token = Read();
      Assert.IsTrue(token.ValueString == "ghi", "Expected token 'ghi' (line 3)");
      Assert.IsFalse(token.IsSet(AstNodeFlags.IsIncomplete), "Expected complete token (line 3)");
      token = Read();
      Assert.IsTrue(token.ValueString == "x", "Expected token 'x' (line 3)");
      Assert.IsFalse(token.IsSet(AstNodeFlags.IsIncomplete), "Expected complete token (line 3)");
      token = Read();
      Assert.IsNull(token, "Null expected.");
    }

  }//class
}//namespace