﻿// See https://aka.ms/new-console-template for more information

// ignore comments like: //
// while, for, for(let a in arr)
// break, continue;
// array adding, deleting, finding, sorting
// object = dictionary (key, value): d={pr1: x, pr2: y, pr3="abc"}; v=d.pr1; v2=d["pr3"];v2=d[a+b]
// 
// todo: make a first standard functions:
// pow(n, m), min(a,b), max(a,b), posibly min(a,....,)
// strToInt, strToDouble, substr, indexOf, 
// todo: make xlsx files modification from script to work
// -------------------- Step2 ------------------------
// todo: make defined functions to work
// class
// make working a.b, a.func(x,y,...)
// connecting scripts in other files
// script libraries
//+ todo: if
//+ array: a = [x, y, 5, "abc"]; v=a[1]; v2=a[i]
//+ array - at run, creation, accessing,

using ExprParser1;
using ScriptRunnerLib;

if (args.Length > 0)
{
    Console.WriteLine("ScriptRunnerTest. The following arguments are specified:");
    int i = 1;
    foreach (var arg in args)
    {
        Console.WriteLine($"Argument{i}={arg}");
        i++;
    }
}
else Console.WriteLine("ScriptRunnerTest. Started...");
testParser();
testScriptRunner();
Console.WriteLine("ScriptRunnerTest. Ended...");

void testParser()
{
    //var ep = new ExprNode("a+func(a,[x,y],b)");
    var ep = new ExprNode("let a=[1,2]");//"a=[x,y]",let b = a[1];
    ep.BuildExprTree();
    string s = ep.DumpToString(false);
    Console.WriteLine(s);
}


void testScriptRunner()
{
    //var res = UtlParserHelper.FindSafeOneOf("//abc//bb,aa", 2, new string[] {"//x", ", "});
    //string s1 = UtlParserHelper.ReplaceManyToOne("abc\n\n\n\nABC\n\n\n", "\n");
    //ExprVar v1 = new ExprVar("a", EType.E_INT, "2", null);
    //ExprVar v2 = new ExprVar("b", EType.E_DOUBLE, "1.2", null);
    //ExprVar v3 = v2.Multiply(v1);

    var sr = new ScriptRunner();
    string script1 = "let a = (17+3*6);\n";
    script1 += "let s=0; for (let i=1; i<10;i=i+1) {s=s+i; }\n";
    script1 += "a = 1; while(   a != 2) { a++;}\n";
    script1 += "  if( a==2) { return;}\n";
    script1 += " function f1(  a, b){let c = a*10;\nlet d=b+10; return c*d;}\n let res = f1(10, 20);";

    string script2 = "let a; a=5; let five2 = pow(a,2);";
    script2 += "function pow(a, n){let res=1; for (let i=0; i<n; i=i+1) {res=res*a; } return res;\n}";

    //string script0 = "{let a=0; a=a+1;out=a;} let a; a=5; let five2 = a*a;let out=0;";
    //string script0 = "let a=1.5; let five2=1; if(a<=0)a=0;else if(a<=1)a=1;else {let a=2.1; if(a<=2)a=2;else a=3;five2 = a*a;} let out=a;";


    //    string script0 = "let a = [2,3,4];let s = 0; for(let i=0; i<3; i=i+1) {s=s+a[i];}";
    string script0 = "let a = [2,3,4];let s = 0; let i=0;" +
        "//while(i<3) {s=s+a[i];if(i==2)break;i=i+1;}" +
        "while(i<3) {s=s+a[i];if(i==2)break;else if(i/2==1){s=s+0;s=s+0;}else{s=s+0;} i=i+1;}\n" +
        "{s=s+a[i];if(i==2)break;//i=i+1;}";
    if (!sr.Parse(script0))
    {
        Console.WriteLine(sr.message);
        return;
    }
    string sDump = sr.Dump(1);
    Console.WriteLine(sDump);
    if (!sr.Run())
    {
        Console.WriteLine(sr.message);
        return;
    }

    var a = sr.GetVar("a");
    var s = sr.GetVar("s");
    var b = sr.GetVar("b");
    //var b1 = sr.GetVar("five2");
    Console.WriteLine($"a={a.ToString()}, five2=b1.ToString(), b={b}");
}