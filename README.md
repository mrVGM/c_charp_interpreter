# Introduction
This is an interpreter for a simple scripting language. It can be integrated into a C# assemblies to execute their methods at runtime. One use case of this
is modding Unity games.

# Syntax
The syntax of the language resembles to the syntax of JavaScript, but it's simpler. Here are some of the base constructs, that can be used:
## Literals
Strings are defined in the usual way, as in most of the other programming languages - enclosed with double quotes. Examples: `"Hello World!"`,
`"There was written \"Hello World!\""`. As you can see, if you need to include double quotes inside your string, you need to escape it with backslash `\`.

Numbers are just defined as sequence with optional fractional part, separated with a dot. Example: `3.14159265`

There are also `true` and `false`, for the 2 boolean values, `null` for the null value in C# and literals `[]` and `{}` for defining an empty list and object, which we will
discuss in the semantics section of this document.

## Variables
A name of a variale can be any string containing the symbols from the latin alphabet, the digits and the underscore symbol `_`, but not starting with a digit.
Examples: `foo`, `BAR`, `foo_123_bar`. A variable, named `foo` is defined with the keyword `let`, just like this: 

`let foo = <some expression>;`

We will clarify what expression is, in a moment. Once defined a variable can be referenced in the code by its name.

## Expressions
Let's first say, what can be considered as a *value* in the context of the language. A value can be any literal, variable, property of an object, accessed by the dot
operator, or the result of a function or method invocation (whose syntax we will explain later). Now expression can be just a value or any arithmetic expression between
values including the operators `+`, `-`, `*`, `/` and paretnthesis. Example: `(1 + 2) * 3`

## Boolean Expressions
Boolean expressions are another kind of constructs, that represents a boolean value. They include comparisons between expressions using the operators `==`, `!=`, `<`, `>`,
`<=`, `>=` and conjunction, disjunction and negation between the comparisons using the operators `&&`, `||` and `!` and parenthesis. Example: `!((1 > 3 || 5 > 4) && 6 < 10)`
Keep in mind that the the `true` and `false` literals are not boolean expressions, according to the definition, they are just expressions. Also, if you have a variable `x` 
that contains boolean value and want to check, if it is true you should use the boolean expression `x == true`.

## Operations and Blocks of operations
There are several constructs in the languages wich we call operations. And all of them ends with semicolon. We have already seem one of them - the declaration of a variable.
Other kinds of operations are:
* `<variable> = <expression>;` or `<value>.<property name> = <expression>;` - an assignment 
* `<exporession>;` - evaluation of expression or invocation of a function in particular 
* `break;` - break statement for getting out of a loop
* `return;`, `return <expression>;` or `return <boolean expression>;` for returning from a function
* blocks of operations and conditional constructs

Now, when we have an idea what operations are, we can easily define a block of operations as a sequence of operations enclosed with curly braces:
```
{ 
  <operation 1>
  <operation 2>
  .
  .
  .
  <operation n> 
}
```

## Conditional constructs
There are only 2 conditional constructs in the language: `if` and `while`. Their syntaxes are like this: `if (<boolean expression>) <block of operations>` and
`while (<boolean expression>) <block of operations>`.
Here is an example of both:
```
let n = 0;
let m = 0;

if (n == 0) {
  m = 5;
}

while (n < 3) {
  m = m + n;
  n = n + 1;
}
```
The values of `m` and `n` after the execution of the code above, should be `8` and `3`.

## Defining Functions
Functions are defined with the keyword `function` like this: `function (<parameters>) <block of operations>`, where paramters are just some names of varaibles separated with
comma. All functions are anonymous by default, to reference a function you need to store it in a variable, like this:
```
let f = function(x) {
  return x + 1;
};
```

## Calling Functions
A function is called, in the same manner as in most of the other programming languages. For example the function `f` defined above, can be called like this: `f(5);`.
However if you want to call a method with template parameters defined in a c# class, you should supply them as well. They need to be passed as strings 
(the full names of the type of the template parameter) or as variables
containing string values to the function like this: 

`<function>|<template parameter 1>|<template paramater 2>|...|<template parameter n>|(<arguments>)`

For example, if you have a variable `go` containing a unity game object and you want to get the Image component attached to it, you can do it like this:

`let image = go.GetComponent|"UnityEngine.UI.Image"|();`

# Semantics
This section will give details on how the interpretator actually works.

## Representations of data
There are 2 base and 2 composite types of data the interpreter works with. The two base ones are string wich is represented as a regular c# string and a number, which is
stored as a custom class containing a double value.
The composite types are a c# `List<object>`, corresponding to the literal `[]`, and a c# `Dict<string, object>`, corresponding to the literal `{}`. The dictionary is used
as a generic object, containing different kinds of values. Almost the same as the objects in JavaScript.

## Scopes
Scopes are containers for the variables, while the interpreter executes a script. Scope is just a dictionary linking a variable name to its value. Actually,
the interpreter works with a hierarchy of scopes. There is a global scope, where some global variables are stored. Every time the interpreter starts to
execute an operations block, it creates a new child scope to the one it currently operates in. When querying the value of a variable, it first checks in the current scope
and if the variable can't be found there, it checks the parent scopes. When declaring a variable it goes right into the current scope. So if for example, you declare a
variable `x` inside the operations block if an `if` construct, it can't be accessed outside it. It is the same with the parameters of functions. Let's look again at the
function `f`, declared in the previous section. When, for example, `f(5)` is called, what happens is:
1. A new child scope of the current one is created
1. The variable `x` (corresponding to the parameter `x` from the function declaration) is created in the new scope
1. The value `5` is assigned to `x`
1. The operations block of the function is executed