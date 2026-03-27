# Updated C Sharp Coding Standards

> **NOTE:** This page is still very much a work in progress.

## Basic Standards

Good clean code should read like a novel. It's recognizable at a glance; well-organized and elegant. As you go through it, you understand its intent without excessive head-scratching or navigating through convoluted logic.
You should aspire to write such code at all times.
The following practices are also prerequisites to progress to intermediate level.

1. **Enough comments** to encourage reuse. Classes and methods which are not easily understood are not reused. Therefore:
   1. All classes must have a comment describing their purpose, unless it would be self-evident even to a Junior Dev new to the project.
   1. All public methods must have a comment describing their purpose.
   1. Any section of code performing non-obvious logic or making non-obvious assumptions should be preceded with a comment describing the logic implemented or assumptions made.
1. **Keep classes short** - As a rule of thumb, any class longer than 350 lines is likely to be too long and trying to do too many things violating the [Single Responsibility Principle](https://malshikay.medium.com/single-responsibility-principle-in-c-62c00a87988a).
1. **Keep methods short and easy to understand** - As a rule of thumb, any method that requires you to scroll vertically on your code editor to view the full method definition is getting too long (though in some cases it can be unavoidable). Most of the time, however, extracting out sections into explicitly named sub methods, will enhance readability significantly.
1. **Order Class Members Logically** - Arrange the members of your class in a logical order to minimize the reader's need to jump around to understand the flow. For instance, group related properties together, placing the most significant ones at the top. If methods are intended to be called in a certain order, their arrangement in the class should mirror this sequence. Sub-methods that are exclusively called by a parent method should be positioned immediately after the parent method, following the logical sequence.
1. **Use #region on long classes** - Using `#region` can help organize your code especially in longer classes. For example, to group the sub methods called by a parent method together.
1. **Use Guard Clauses** - Use Guard clauses to Test for all the necessary preconditions for a complex method to execute up-front is a great way to clean up your code. It not only clearly communicates the preconditions and expectations are for anyone calling your method, but also simplifies the code in the body of the method as it removes the need to perform checks further down. [See](https://www.youtube.com/watch?v=UUehR_VNyro)
   - Use [Ardalis.GaurdClauses](https://github.com/ardalis/GuardClauses) as a standard library
1. **Excessive nesting** - Excessive nesting through nested conditional (`if`, `switch`) and looping (`while`, `foreach`) statements Can make the code difficult to read. If you are nesting three layers deep or more you should probably consider refactoring. Extracting out a nested section into a separate method can usually help, as well as applying the early return principle.
1. **Proper variable and method naming** - Ensure you use clear variable and method names (without spelling mistakes!) and be consistent in your naming conventions. Sometimes it can take time to find just the right name that clearly conveys the correct meaning and intent, but it pays dividends in the long term through improved clarity and maintainability. Don't rush this!
1. **Replace 'Magic numbers' with constants or enums** - [See example](https://refactoring.guru/replace-magic-number-with-symbolic-constant).
1. **DRY - Don't Repeat Yourself** - 'Copy and paste coding' is lazy and rude, and creates bloat that is difficult to maintain in the long term. If your code looks similar to another section, extract out the common logic into a reusable method.
1. **Simple and Direct** - Adhering to the KISS (Keep It Simple Stupid) principle, well-written code should be as uncomplicated as possible, facilitating easy understanding and debugging. Generally, the fewer lines of code required to accomplish a task, the better. However, there are exceptions, such as when a single conditional statement or Linq statements attempts to incorporate too many conditions, making it convoluted. In such cases, it's preferable to expand the code to make it easier to understand.
1. **Format your code** - Visual Studio tends to format your code automatically, but poorly formatted code still makes its way into the repos which betrays a lack of attention to detail. Use these shortcuts to do it effortlessly.
   - `Ctrl+E, Ctrl+D` to format the entire document.
   - `Ctrl+E, Ctrl+F` to format the selection.
1. **Remove dead code** - It is sometimes scary deleting sections of code especially if you did not write it but any code which is no longer utilized still adds clutter and confusion which hinders maintainability. If it is no longer used delete it!

### Useful resources:

- [5 Awesome C# Refactoring Tips](https://www.milanjovanovic.tech/blog/5-awesome-csharp-refactoring-tips)
- (10 Coding Principles Explained in 5 Minutes)[https://www.youtube.com/watch?v=GmXPwRNIrAU]
- [The Art of Code Refactoring in C#](https://medium.com/@kerimkkara/the-art-of-code-refactoring-in-c-d02f0346a1dd)
- [Refactoring Course](https://refactoring.guru/refactoring/course) - We'll sponsor the cost of the course for anyone who wants to complete this!
- [Clean Code presentation from Uncle Bob](https://www.youtube.com/watch?v=7EmboKQH8lM&list=PLmmYSbUCWJ4x1GO839azG_BBw8rkh-zOj) - A bit long but easy to watch and full of wisdom

## Efficient

Good code should be efficient and free from obvious performance issues. The most common performance issues usually include:

- Using loops that cause excessive database calls instead of consolidating them into a single SQL query when possible. For example:
  - Looping through, causing multiple SELECT statements to the database where a 'flattened entity' joining the appropriate tables would reduce this to a single SELECT call.
  - Looping through to make multiple updates instead of a single bulk update statement.
- Not performing database indexing for queries expected to be made often by the application.

## Solution Structure

- Always follow the solution templates which follows the Clean Architecture structure
- Always place classes the right

### Common Mistakes

- Domain logic in the AppService
- Placing DTO classes in the domain layer:
  - Because DTO classes often look very much like entity classes, less experienced developers are often tempted to place DTOs in the domain project. This should not be the case as a matter of course. DTOs should be placed under a `Dtos` folder next to the AppService which uses. There can be some exceptions when a Dto is used as a parameter to a domain service, in which case it is acceptable to have the Dto place in the same folder as the DomainService.
  - When creating a Dto that serves a single function (e.g create or update or response only), the naming convention should take on the following format `{end-pointname}Request` e.g `CreateFieldServiceProfileRequest`.

---

# Updated NextJs Best Practices

## Tips for Clean and Efficient Code

### Utilize File-Based Routing

Next.js employs a file-based routing system, allowing you to create pages simply by adding React components in the “pages” directory. Adhering to this convention not only makes your project structure more organized but also simplifies the management of routes and improves code maintainability.

### Leverage Server-Side Rendering Wisely

While server-side rendering enhances SEO and initial page load times, it may not be necessary for all pages. Identify pages that require SSR, such as dynamic or content-heavy pages, and use Next.js’s “getServerSideProps” or “getInitialProps” functions selectively for optimal performance.

### Embrace Static Site Generation (SSG)

Static Site Generation offers better performance and scalability compared to SSR for pages with static content. For pages that do not require real-time data, leverage SSG with “getStaticProps” to generate static HTML files at build time and serve them directly to users, reducing server load.

### Optimize Image Loading

Images can significantly impact page load times. Use Next.js’s “Image” component, which automatically optimizes images and provides lazy loading, ensuring faster rendering and improved performance.

### Code Splitting and Dynamic Imports

Take advantage of Next.js’s built-in code splitting feature to divide your application code into smaller, more manageable chunks. Use dynamic imports to load non-essential components only when needed, reducing the initial bundle size and improving overall page load times.

### Minimize Third-Party Dependencies

Be cautious when adding third-party libraries and packages to your project, as they can increase the bundle size and affect performance. Opt for lightweight alternatives or, where feasible, write custom solutions to reduce dependencies.

### Manage State Effectively

Select the appropriate state management solution for your project, such as React’s built-in “useState” and “useReducer” hooks or external libraries like Redux or Zustand. Keep the global state minimal, and prefer passing data between components using props whenever possible.

### Opt for TypeScript Integration

Integrating TypeScript in your Next.js project provides static type-checking, reducing the chances of runtime errors and enhancing code reliability. Embrace TypeScript to improve code maintainability and collaboration within your team.

### Properly Handle Error States

Handle error states gracefully by implementing custom error pages using Next.js’s “ErrorBoundary” or the “getStaticProps” and “getServerSideProps” functions. Providing users with informative error messages enhances user experience and helps identify and resolve issues quickly.

### Implement Caching Strategies

Leverage caching techniques, such as HTTP caching and data caching, to reduce server requests and enhance performance. Caching can significantly improve response times for frequently requested resources.

---

# Updated NextJs Code Style

# Code Style Guidelines

### Code should be functional in style rather than Object Orientated or Imperative unless there are no clean alternatives.

- Use pure functions where possible to make them testable and modular.
- Avoid mutating variables and the let keyword.
- Prefer functional & immutable array methods .ie map/filter/reduce/some/every over any types of mutable for loop.
- Prefer `return early` coding style, more about it here.
- Avoid classes and stateful modules where possible.
- Don't Repeat Yourself. Make extensive use of the constants and utils files for re-usable strings and methods.
- Don't obsess over performance of code, obsess over making it clear.
- The above rules can be relaxed for test scripts.

### React components should be simple and composable and cater to real life UI design problems:

- `Simplicity`: Strive to keep the component API fairly simple and show real-world scenarios of using the component.
- `Representational`: React components should be templates, free of logic, and purely presentational. It aims to make our components shareable and easy to test.
- `Composition`: Break down components into smaller parts with minimal props to keep complexity low, and compose them together. This will ensure that the styles and functionality are flexible and extensible.
- `Accessibility`: When creating a component, keep accessibility top of mind. This includes keyboard navigation, focus management, color contrast, voice over, and the correct aria-\* attributes.
- `Naming Props`: We all know naming is the hardest thing in this industry. Generally, ensure a prop name is indicative of what it does. Boolean props should be named using auxiliary verbs such as does, has, is and should. For example, Button uses `isDisabled`, `isLoading`, etc.

### When doing styling, strive to use `tailwindcss` classes for consistency and painless maintenance.

- Avoid hard coding colors. All colors from the design guideline should be pre-defined in tailwind.js. Please use them with respect.
- Try to avoid injecting styles via style attribute because it's impossible to build responsiveness with inline styles. Strive to only use Tailwind classes for sizing, spacing, and building grid layouts.

// Don't

    <div style={{marginLeft: '8px'}}></div>

// Do

    <div className="ml-2"></div

// Grid layout

    <div className="grid grid-cols-3 gap-5">
      <div className="col-span-2"></div>
      <div className="col-span-1"></div>
    </div>

### Maintain the separation of concerns in the folder structure laid out in the initial scaffold:

- `constants/` contains shared variables used across the app.
- `components/` contains shared ui components used across the app.
- `utils/` contains shared functions used across the app.
- `hooks/` contains shared hooks used across the app.
- `types/` contains common TypeScript types or interfaces.

### When adding third party libraries to the project consider carefully the following;

- Is this a trivial package we can easily write in-house? If so, take the time to roll your own.
- Is this library well supported, under active development and widely used in the community? If not, do not use it.
- Do we use a similar but different library for the same task elsewhere in the company? If so, use this library for developer familiarity and consistency.
- Will this library impact significantly performance or bundle size eg Lodash or Moment? If so, consider carefully if it is necessary or if there is a better alternative.

### TypeScript helps write code better but should be used with care to not lose its strengths:

- Avoid using any when possible. Using any is sometimes valid, but should rarely be used, even if to make quicker progress. Even Unknown is better than using any if you aren't sure of an input parameter.
- Pay attention when using the non-null assertion operator !. Only use if you know that a variable cannot be null right now rather than blindly using it to pass the validation step.

### File naming should be PascalCase for all React components files and kebab-case for the rest.

Exports from files can be either as variable or default exports, but please stick to naming the object before using export default to avoid anonymous module names in stack traces and React dev tools.

Your app should be fast but also remember **Premature optimisation is the root of all evil**. If you think it’s slow, prove it with a benchmark., the profiler of React Developer Tools (Chrome extension) is your friend!

### General suggestions

- Use `useMemo` mostly just for expensive calculations.
- Use `React.memo`, `useMemo`, and `useCallback` for reducing re-renders, they shouldn't have many dependencies and the dependencies should be mostly primitive types.
- Make sure your `React.memo`, `useCallback` or `useMemo` is doing what you think it's doing (is it really preventing re-rendering?).
- Stop punching yourself every time you blink (fix slow renders before fixing re-renders).
- Putting your state as close as possible to where it's being used will not only make your code so much easier to read but It would also make your app faster (state colocation).
- Context should be logically separated, do not add to many values in one context provider. If any of the values of your context changes, all components consuming that context also re-renders even if those components don't use the specific value that was actually changed.
- You can optimize context by separating the `state` and the `dispatch` function.
- Know the terms `lazy loading` and `bundle/code splitting`
- A smaller bundle size usually also means a faster app. You can visualize the code bundles you've generated with `@next/bundle-analyzer`.

---

# Updated JavaScript Coding Standards

# **Coding Standards JavaScript / TypeScript**

## Quick Reference

### Classes:

Class name:

- Pascal Case
- Must be documented
- Add regions for:
  > - Constants
  > - Private variables
  > - Public variables
  > - Constructor
  > - Public methods
  > - Private methods

Constants:

- ALL UPPER CASE
- Must be documented

Private Variables:

- Underscore `_`, followed by Camel Case name
- Must be preceded with `private` keyword

Public Variables:

- Camel Case
- Must be documented
- Must be preceded with `public` keyword

Public Methods:

- Camel Case
- Must be documented
- Must be preceded with `public` keyword

Private Methods:

- Camel Case
- Must be preceded with `private` keyword

Functions & Methods (general):

- Camel Case
- Must declare return type
- All parameters must declare type

---

### Interfaces:

Interface name:

- Capital `I` followed by Pascal Case name
- Must be documented

Variables & Functions:

- Camel Case
- Must be documented

---

### General Rules:

- Use of `any` variable/function type is **not** allowed
- Variables may only contain alpha-numeric characters as well as an underscore (restricted to private class-scope variables)
- Comments `//` followed by space and starts with lower case
- Opening curly brackets must start on the same line
- No spaces after closing curly bracket or semi colon
- Indent: 4 spaces (no tabs)
- Maximum line length: 200 characters
- No consecutive blank lines
- No empty blocks of code. Add `// todo: description` to the block if code will be implemented later
- Use of `eval` is not allowed
- Use of object properties through string literals is restricted, eg. `myVar[“propName”]`
- No Switch-case fall-through
- No unreachable code allowed
- No unused expressions/variables/functions allowed
- Variables & functions must be declared before they are used
- Single statements following a control block `if/for/while/do` must be enclosed within curly brackets `{ … }`
- Only double-quotes may be used for strings
- All comparison operators must use triple comparison: `===`, `!==`
- All modules must begin with `use strict`
- **Do not** use abbreviations on any variable/method name
- File names should be the same as the class/function name that is implemented inside it

---

## Indentation

The unit of indentation is four spaces. Use of tabs should be avoided.

---

## Line Length

Avoid lines longer than 200 characters. When a statement will not fit on a single line, it may be necessary to break it. Place the break after an operator, ideally after a comma. A break after an operator decreases the likelihood that a copy-paste error will be masked by semicolon insertion. The next line should be indented double the previous line’s spaces.

---

## Comments

Be generous with comments. It is useful to leave information that will be read at a later time by people (possibly yourself) who will need to understand what you have done. The comments should be well-written and clear, just like the code they are annotating.

Comments inside functions should start with `//`, followed by a single space. The text should always start with a lower case. Example:

    // this is my comment

Documentation comments should start with `/** ` with text placed on a new line.  
The comment should be closed with `*/` with no text preceding the comment. Example:

    /**
     * Documentation text…
     * Some more documentation text…
     */

Note that the alignment of the stars `*` is important for documentation generation to happen properly.
Function parameters must also be documented with the following format:

    /**
     * Function description…
     * @param ParameterName1 Parameter 1 description…
     * @param ParameterName2 Parameter 2 description…
     */

---

## Variable Declarations

All variables should be declared before used. Use of global variables should be minimised.

Variable declarations should always include a variable type. The use of `any` is prohibited unless properly documented with a reason for its use.

It is preferred that each variable be given its own line and comment. It is recommended that variables be initialised by assigning it a default value.

    var currentEntry: string = ""; // currently selected table entry
    var level: number = 0; // indentation level
    var size: number = 10; // size of table
    var obj: SomeObj = null; // description...

JavaScript does not have block scope, so defining variables in blocks can confuse programmers who are experienced with other C family languages. Define all variables at the top of the function.

---

## Function Declarations

All functions should be declared before they are used. Inner functions should follow the var statement. This helps make it clear what variables are included in its scope.

There should be no space between the name of a function and the ( (left parenthesis) of its parameter list. There should be one space between the ) (right parenthesis) and the { (left curly brace) that begins the statement body. The body itself is indented four spaces. The } (right curly brace) is aligned with the line containing the beginning of the declaration of the function.

    function outer(c: number, d: number): number {
        var e: number = c * d;

        function inner(a: number, b: number): number {
            return (e * a) + b;
        }

        return inner(0, 1);
    }

Functions should always declare a return type. If it returns nothing, it should be declared as a `void` function.

    function test(): void {
    // …
    }

All function parameters should be declared with a specific type. The use of `any` is prohibited unless well documented with a reason for its use.

If a function literal is anonymous, there should be one space between the word function and the ( (left parenthesis). If the space is omitted, then it can appear that the function's name is function, which is an incorrect reading. Again, anonymous functions should always declare a return type.

    div.onclick = function (e): boolean {
        return false;
    };

    that = {
        method: function (): number {
            return this.datum;
        },
        datum: 0
    };

Use of global functions are restricted – all functions must be contained within a proper namespace.

If a function is declared inside a class, it should always indicate whether or not it is a `private` or `public` function.

When a function is to be invoked immediately, the entire invocation expression should be wrapped in parens so that it is clear that the value being produced is the result of the function and not the function itself.

    var collection = (function (): Object {
    	var keys: string[] = [], values: string[] = [];

    	return {
    		get: function (key: string): string {
    			var at: number = keys.indexOf(key);
    			if (at >= 0) {
    				return values[at];
    			}
    		},
    		set: function (key: string, value: string): void {
    			var at: number = keys.indexOf(key);
    			if (at < 0) {
    				at = keys.length;
    			}
    			keys[at] = key;
    			values[at] = value;
    		},
    		remove: function (key: string): void {
    			var at: number = keys.indexOf(key);
    			if (at >= 0) {
    				keys.splice(at, 1);
    				values.splice(at, 1);
    			}
    		}
    	};
    });

---

## Names

Names should be formed from the 26 upper and lower case letters (A .. Z, a .. z), the 10 digits (0 .. 9), and \_ (underbar). Avoid use of international characters because they may not read well or be understood everywhere. **Do not** use `$` (dollar sign) or `\` (backslash) in names.

The use of the underscore `_` is restricted to indicating private class-scope variables. It should not be used anywhere else.

Most variables and functions should start with a lower case letter.

Global variables should be in all caps. (JavaScript does not have macros or constants, so there isn't much point in using all caps to signify features that JavaScript doesn't have.)

---

## Class Declarations

Class names should always use Pascal case (start with a capital letter, with each letter of a new word also starting with a capital letter).

Only if classes are to be reused by other (external) components, should they be preceded with the `export` keyword.

Class sections should always be declared in the following order:

- Constants
- Private variables
- Public variables
- Constructor
- Public methods
- Private methods

Private variables should always start with an underscore. The rest of the variable name should be written in Camel case.

Public variables, Public methods and Private methods should all be written in Camel case.

Constants must be written in all CAPS.

It is preferable that all sections should be contained within a REGION section for easy collapsing. The start and end of the regions should include the same description in order for it to display correctly when collapsed.

    /**
     * My class documentation…
     */

    export class MyClass {
    	// #region constants
    		public MYCONSTANT: number = 100;
    	// #endregion constants

    	// #region public variables
    		public someString: string;
    	// #endregion public variables

    	// #region private variables
    		private _someNumber: number;
    	// #endregion private variables

    	Constructor(param1: number) { }

    	// #region public methods
    	/**
    	 * Method description…
    	 * @param parameter1 Parameter 1 description…
    	 */

    	public someMethod1(parameter1: string): number {
    		// …
    	}

    	// #endregion public methods

    	// #region private methods

    	/**
    	 * Method description…
    	 * @param parameter1 Parameter 1 description…
    	 */
    	private someMethod1(parameter1: string): number {
    		// …
    	}
    	// #endregion private methods
    }

---

## Interface Declarations

Interfaces are never compiled to JavaScript. The use of interface is to enhance productivity by adding intelli-sense to objects that would otherwise be seen as “any” objects. It also assists in design-time compilation and error elimination.

Interfaces should always start with a capital `I`, followed by the Pascal-case name of the interface.
All interface properties and methods should be properly documented.

    export interface IMyInterface {
    	/**
    	 * Variable description…
    	*/
    	myVar: number;
    }

To indicate that a property or method on an interface is optional (in other words, it might not exist on the object that implements the interface), a question mark can be added after the variable name.

    myVar?: number;

---

## Enums

Enums are compiled into JavaScript arrays that can return strings or numbers. Enum names should be written in Camel Case, and should always be documented, as well as each value within the Enum. Each value should always include a specific numeric value – undefined values (that are created by the compiler) should be avoided.

    /**
     * Enum description…
     */

    export enum MyEnum {
    	/**
    	 * description…
    	 */
    	a = 1,
    	/**
    	 * description…
    	 */
    	b = 2
    }

Accessing the numeric value of an Enum uses the following format:

    var enumVal: number = MyEnum[MyEnum.a];

---

## Regions

Regions should be used to group sections of code that logically belong together.

A region always starts with two forward-slashes, followed by a space and then the keyword `#region` followed by a space and the name or description of the region.

Regions must always be enclosed with the `#endregion` keyword.

    // #region My Region Name
       …code…
    // #endregion My Region Name

It is convenient to start and end the region with the same name. This will ensure that the region name stays visible when the region is collapsed.

---

## Statements

### Simple Statements

Each line should contain at most one statement. Put a `;` (semicolon) at the end of every simple statement. Note that an assignment statement that is assigning a function literal or object literal is still an assignment statement and must end with a semicolon.

JavaScript allows any expression to be used as a statement. This can mask some errors, particularly in the presence of semicolon insertion. The only expressions that should be used as statements are assignments and invocations.

### Compound Statements

Compound statements are statements that contain lists of statements enclosed in `{ }` (curly braces).

- The enclosed statements should be indented four more spaces.
- The `{` (left curly brace) should be at the end of the line that begins the compound statement.
- The `}` (right curly brace) should begin a line and be indented to align with the beginning of the line containing the matching `{` (left curly brace).
- Braces should be used around all statements, even single statements, when they are part of a control structure, such as an if or for statement. This makes it easier to add statements without accidentally introducing bugs.

### Labels

Statement labels are optional. Only these statements should be labeled: `while, do, for, switch`.

`return` Statement

A `return` statement with a value should not use `( )` (parentheses) around the value. The return value expression must start on the same line as the return keyword in order to avoid semicolon insertion.

`if` Statement

The `if` class of statements should have the following form:

    if (condition) {
        statements
    }

    if (condition) {
        statements
    } else {
        statements
    }

    if (condition) {
        statements
    } else if (condition) {
        statements
    } else {
        statements
    }

`for` Statement

A `for` class of statements should have the following form:

    for (initialisation; condition; update) {
    	statements
    }

    for (variable in object) {
        if (filter) {
            statements
        }
    }

Variables used inside the `for` statement should always be declared before it is used. It should never be declared in-line.

The first form should be used with arrays and with loops of a predetermined number of iterations.

The second form should be used with objects. Be aware that members that are added to the prototype of the object will be included in the enumeration. It is wise to program defensively by using the `hasOwnProperty` method to distinguish the true members of the `object`:

    for (variable in object) {
        if (object.hasOwnProperty(variable)) {
            statements
        }
    }

`while` Statement

A `while` statement should have the following form:

    while (condition) {
        statements
    }

`do` Statement

A `do` statement should have the following form:

    do {
        statements
    } while (condition);

Unlike the other compound statements, the `do` statement always ends with a `;` (semicolon).

`switch` Statement

A `switch` statement should have the following form:

    switch (expression) {
        case expression:
            statements
        default:
            statements
    }

Each group of statements (except the default) should end with `break, return, or throw`. Do not fall through.

`try` Statement

The `try` class of statements should have the following form:

    try {
        statements
    } catch (variable) {
        statements
    }

    try {
        statements
    } catch (variable) {
        statements
    } finally {
        statements
    }

`continue` Statement

Avoid use of the `continue` statement. It tends to obscure the control flow of the function.

`with` Statement

The `with` statement should not be used.

---

## Whitespace

Blank lines improve readability by setting off sections of code that are logically related.

Blank spaces should be used in the following circumstances:

- A keyword followed by `(` (left parenthesis) should be separated by a space.
  `while (true) {`
- A blank space should not be used between a function value and its `(` (left parenthesis). This helps to distinguish between keywords and function invocations.
- All binary operators except `.` (period) and `(` (left parenthesis) and `[` (left bracket) should be separated from their operands by a space.
- No space should separate a unary operator and its operand except when the operator is a word such as `typeof`.
- Each `;` (semicolon) in the control part of a for statement should be followed with a space.
- Whitespace should follow every `,` (comma).
- There should be **NO** space after closing curly brackets or semi-colons. These usually indicate the end of a statement or function or class and all code following should be placed on a new line.

---

## Compiler Directives

### Debug Includes

To include code for debugging purposes only, the following directives can be used:

Single line:

    /*IFDEBUG*/ …code…

Multi-line:

    // #region IFDEBUG
    … code …
    // #endregion IFDEBUG

If the debugging switch is turned off (Project > Properties > Build), all statements associated with the debug directives will be stripped from the code before the file is compiled.

### Content Embedding

To embed content into TypeScript files before they are compiled, the following directive can be used:

To include JavaScript:

    /*JSINJECTSTART:file/*JSINJECTEND*/

In this case, `file` is replaced with the relative path to the file that must be embedded.
Note that the file is injected as-is – not pre-formatting is applied to the file before it is embedded.

To include a HTML file:

    /*HTMLINJECTSTART:file/*HTMLINJECTEND*/

In this case, `file` is replaced with the relative path to the file that must be embedded.

Note that all quotes found in the file are escaped, and Line Feeds and New Line characters are removed, before the file is embedded.

---

## Bonus Suggestions

`{}` and `[]`
Use `{}` instead of `new Object()`. Use `[]` instead of `new Array()`.

Use arrays when the member names would be sequential integers. Use objects when the member names are arbitrary strings or names.

`,` (comma) Operator

Avoid the use of the `,` (comma) operator. (This does not apply to the comma separator, which is used in object literals, array literals, var statements, and parameter lists.)

### Block Scope

In JavaScript blocks do not have scope. Only functions have scope. Do not use blocks except as required by the compound statements.

### Assignment Expressions

Avoid doing assignments in the condition part of `if` and `while` statements.

Is

    if (a = b) {

a correct statement? Or was

    if (a == b) {

intended? Avoid constructs that cannot easily be determined to be correct.

`===` and `!==` Operators.
Only use the `===` and `!==` operators. The `==` and `!=` operators do type coercion and should not be used.

### Confusing Pluses and Minuses

Be careful to not follow a `+` with `+` or `++`. This pattern can be confusing. Insert parens between them to make your intention clear.

    total = subtotal + +myInput.value;

is better written as

    total = subtotal + (+myInput.value);

so that the `+` `+` is not misread as `++`.

`eval` is Evil
The `eval` function is the most misused feature of JavaScript. Avoid it.
`eval` has aliases. Do not use the Function constructor. Do not pass strings to `setTimeout` or `setInterval`.

---

# Provider Pattern Contract (Strict)

## Purpose

This file defines the exact provider setup style required in this frontend.

## Required Structure for Every Provider

```text
providers/<feature>Provider/
  actions.tsx
  context.tsx
  index.tsx
  reducer.tsx
```

This structure is mandatory. Do not deviate.

## Canonical Example (Auth Provider)

### `actions.tsx`

```tsx
import { createAction } from "redux-actions";
import { IProduct, IProductStateContext } from "./context";

// Enum defining the type of actions that can be dispatched
export enum ProductActionEnums {
  // Each operation (get/create/update/delete) has three states:
  // PENDING: Action started
  // SUCCESS: Action completed successfully
  // ERROR: Action failed
  getProductsPending = "GET_PRODUCTS_PENDING",
  getProductsSuccess = "GET_PRODUCTS_SUCCESS",
  getProductsError = "GET_PRODUCTS_ERROR",

  getProductPending = "GET_PRODUCT_PENDING",
  getProductSuccess = "GET_PRODUCT_SUCCESS",
  getProductError = "GET_PRODUCT_ERROR",

  createProductPending = "CREATE_PRODUCT_PENDING",
  createProductSuccess = "CREATE_PRODUCT_SUCCESS",
  createProductError = "CREATE_PRODUCT_ERROR",

  updateProductPending = "UPDATE_PRODUCT_PENDING",
  updateProductSuccess = "UPDATE_PRODUCT_SUCCESS",
  updateProductError = "UPDATE_PRODUCT_ERROR",

  deleteProductPending = "DELETE_PRODUCT_PENDING",
  deleteProductSuccess = "DELETE_PRODUCT_SUCCESS",
  deleteProductError = "DELETE_PRODUCT_ERROR",
}

// createAction<PayloadType>(actionType, payloadCreator)
// - PayloadType: The type of data the action will contain
// - actionType: The string identifier for this action
// - payloadCreator: Function that returns the action payload

// Get All Products Actions
export const getProductsPending = createAction<IProductStateContext>(
  ProductActionEnums.getProductsPending,
  // Returns state object indicating loading started
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

// Example of createAction with multiple generic types:
// createAction<ReturnType, PayloadType>
export const getProductsSuccess = createAction<
  IProductStateContext, // What the payload creator returns
  IProduct[] // Type of argument passed to payload creator
>(
  ProductActionEnums.getProductsSuccess,
  // Receives products array and returns state with products
  (products: IProduct[]) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    products, // Include fetched products in state
  }),
);

export const getProductsError = createAction<IProductStateContext>(
  ProductActionEnums.getProductsError,
  // Returns state object indicating error occurred
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

// Single Product Actions
// Similar pattern: each action updates the state to reflect the operation status
export const getProductPending = createAction<IProductStateContext>(
  ProductActionEnums.getProductPending,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const getProductSuccess = createAction<IProductStateContext, IProduct>(
  ProductActionEnums.getProductSuccess,
  (product: IProduct) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    product,
  }),
);

export const getProductError = createAction<IProductStateContext>(
  ProductActionEnums.getProductError,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const createProductPending = createAction<IProductStateContext>(
  ProductActionEnums.createProductPending,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const createProductSuccess = createAction<
  IProductStateContext,
  IProduct
>(ProductActionEnums.createProductSuccess, (product: IProduct) => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  product,
}));

export const createProductError = createAction<IProductStateContext>(
  ProductActionEnums.createProductError,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const updateProductPending = createAction<IProductStateContext>(
  ProductActionEnums.updateProductPending,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const updateProductSuccess = createAction<
  IProductStateContext,
  IProduct
>(ProductActionEnums.updateProductSuccess, (product: IProduct) => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  product,
}));

export const updateProductError = createAction<IProductStateContext>(
  ProductActionEnums.updateProductError,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const deleteProductPending = createAction<IProductStateContext>(
  ProductActionEnums.deleteProductPending,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const deleteProductSuccess = createAction<
  IProductStateContext,
  IProduct
>(ProductActionEnums.deleteProductSuccess, (product: IProduct) => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  product,
}));

export const deleteProductError = createAction<IProductStateContext>(
  ProductActionEnums.deleteProductError,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);
```

### `context.tsx`

```tsx
import { createContext } from "react";

// Interface defining the shape of a Product object
// This represents the data structure we expect from the API
export interface IProduct {
  id: string;
  title: string;
  price: number;
  description: string;
  image: string;
  category: string;
}

// Interface defining the state shape for our context
// This includes status flags and the actual product data
export interface IProductStateContext {
  isPending: boolean; // Loading state
  isSuccess: boolean; // Success state
  isError: boolean; // Error state
  product?: IProduct; // Single product data (optional)
  products?: IProduct[]; // Array of products (optional)
}

// Interface defining all the actions that can be performed on our products
// These methods will be implemented in the provider component
export interface IProductActionContext {
  getProducts: () => void; // Fetch all products
  getProduct: (id: string) => void; // Fetch a single product
  createProduct: (product: IProduct) => void; // Create a new product
  updateProduct: (product: IProduct) => void; // Update existing product
  deleteProduct: (id: string) => void; // Delete a product
}

// Initial state object that defines the default values for our product context
export const INITIAL_STATE: IProductStateContext = {
  isPending: false, // Indicates if a request is in progress
  isSuccess: false, // Indicates if the last operation was successful
  isError: false, // Indicates if the last operation resulted in an error
};

// Create two separate contexts:
// 1. ProductStateContext - Holds the current state of our products
export const ProductStateContext =
  createContext<IProductStateContext>(INITIAL_STATE);

// 2. ProductActionContext - Holds the methods to interact with our products
export const ProductActionContext =
  createContext<IProductActionContext>(undefined);
```

### `index.tsx`

```tsx
import { getAxiosInstace } from "../../utils/axiosInstance";
import {
  INITIAL_STATE,
  IProduct,
  ProductActionContext,
  ProductStateContext,
} from "./context";
import { ProductReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getProductsError,
  getProductsPending,
  getProductsSuccess,
  getProductError,
  getProductPending,
  getProductSuccess,
  createProductPending,
  createProductError,
  updateProductSuccess,
  createProductSuccess,
  updateProductPending,
  updateProductError,
  deleteProductPending,
  deleteProductSuccess,
  deleteProductError,
} from "./actions";
import axios from "axios";

export const ProductProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ProductReducer, INITIAL_STATE);
  const instance = getAxiosInstace();

  const getProducts = async () => {
    dispatch(getProductsPending());
    const endpoint = `https://fakestoreapi.com/products`;
    await axios(endpoint)
      .then((response) => {
        dispatch(getProductsSuccess(response.data));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getProductsError());
      });
  };

  const getProduct = async (id: string) => {
    dispatch(getProductPending());
    const endpoint = `/products/${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getProductSuccess(response.data));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getProductError());
      });
  };

  const createProduct = async (product: IProduct) => {
    dispatch(createProductPending());
    const endpoint = `/products`;
    await instance
      .post(endpoint, product)
      .then((response) => {
        dispatch(createProductSuccess(response.data));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createProductError());
      });
  };

  const updateProduct = async (product: IProduct) => {
    dispatch(updateProductPending());
    const endpoint = `/products/${product.id}`;
    await instance
      .put(endpoint, product)
      .then((response) => {
        dispatch(updateProductSuccess(response.data));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateProductError());
      });
  };

  const deleteProduct = async (id: string) => {
    dispatch(deleteProductPending());
    const endpoint = `https://fakestoreapi.com/products/${id}`;
    await instance
      .delete(endpoint)
      .then((response) => {
        dispatch(deleteProductSuccess(response.data));
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteProductError());
      });
  };

  return (
    <ProductStateContext.Provider value={state}>
      <ProductActionContext.Provider
        value={{
          getProducts,
          getProduct,
          createProduct,
          updateProduct,
          deleteProduct,
        }}
      >
        {children}
      </ProductActionContext.Provider>
    </ProductStateContext.Provider>
  );
};

export const useProductState = () => {
  const context = useContext(ProductStateContext);
  if (!context) {
    throw new Error("useProductState must be used within a ProductProvider");
  }
  return context;
};

export const useProductActions = () => {
  const context = useContext(ProductActionContext);
  if (!context) {
    throw new Error("useProductActions must be used within a ProductProvider");
  }
  return context;
};
```

### `reducer.tsx`

```tsx
import { handleActions } from "redux-actions";
import { INITIAL_STATE, IProductStateContext } from "./context";
import { ProductActionEnums } from "./actions";

export const ProductReducer = handleActions<
  IProductStateContext,
  IProductStateContext
>(
  {
    [ProductActionEnums.getProductsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.getProductsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.getProductsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.getProductPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.getProductSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.getProductError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.createProductPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.createProductSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.createProductError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.updateProductPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.updateProductSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.updateProductError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.deleteProductPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.deleteProductSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ProductActionEnums.deleteProductError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE,
);
```

## Required Page/Provider Composition Style

`````tsx
import { useEffect } from "react";
import { Button, Card, Empty, Spin, Typography } from "antd";
import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import {
  useProductActions,
  useProductState,
} from "../../providers/productProvider";
import { IProduct } from "../../providers/productProvider/context";
import { useStyles } from "./style";

const { Meta } = Card;
const { Text } = Typography;

const ProductsPage = () => {
  const { styles } = useStyles();

  const { products, isPending, isError } = useProductState();
  const { getProducts, deleteProduct, updateProduct } = useProductActions();

  useEffect(() => {
    getProducts();
  }, [getProducts]);

  const handleDelete = async (id: string) => {
    try {
      await deleteProduct(id);
    } catch (error) {
      console.error("Failed to delete product:", error);
    }
  };

  const handleEdit = async (product: IProduct) => {
    try {
      await updateProduct(product);
    } catch (error) {
      console.error("Failed to update product:", error);
    }
  };

  if (isPending) {
    return (
      <div className={styles.stateWrapper}>
        <Spin size="large" />
        <Text className={styles.stateText}>Loading products...</Text>
      </div>
    );
  }

  if (isError) {
    return (
      <div className={styles.stateWrapper}>
        <Text className={styles.errorText}>Error loading products!</Text>
      </div>
    );
  }

  if (!products || products.length === 0) {
    return (
      <div className={styles.stateWrapper}>
        <Empty
          description={
            <span className={styles.stateText}>No products found</span>
          }
        />
      </div>
    );
  }

  return (
    <div className={styles.productsGrid}>
      {products.map((product) => (
        <Card
          key={product.id}
          className={styles.productCard}
          cover={
            <div className={styles.imageWrapper}>
              <img
                alt={product.title}
                src={product.image}
                className={styles.productImage}
              />
            </div>
          }
          actions={[
            <Button
              key={`edit-${product.id}`}
              className={styles.actionBtn}
              icon={<EditOutlined />}
              onClick={() => handleEdit(product)}
            >
              Edit
            </Button>,
            <Button
              key={`delete-${product.id}`}
              danger
              className={styles.actionBtn}
              icon={<DeleteOutlined />}
              onClick={() => handleDelete(product.id)}
            >
              Delete
            </Button>,
          ]}
        >
          <Meta
            title={<span className={styles.productTitle}>{product.title}</span>}
            description={
              <div className={styles.metaContent}>
                <Text className={styles.priceText}>
                  ${product.price?.toFixed(2)}
                </Text>
                <Text className={styles.categoryText}>{product.category}</Text>
              </div>
            }
          />
        </Card>
      ))}
    </div>
  );
};

export default ProductsPage;
```

## Required `antd-style` Pattern Example

````tsx
import { createStyles, css } from "antd-style";

export const useStyles = createStyles(({ token }) => ({
  productsGrid: css`
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 16px;
    width: 100%;
  `,

  productCard: css`
    &.ant-card {
      background: rgba(58, 63, 71, 0.38) !important;
      backdrop-filter: blur(10px);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 20px;
      overflow: hidden;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.18);
      transition:
        transform 0.2s ease,
        box-shadow 0.2s ease,
        border-color 0.2s ease;
    }

    &.ant-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 32px rgba(0, 0, 0, 0.24);
      border-color: rgba(255, 255, 255, 0.14);
    }

    .ant-card-body {
      background: transparent;
    }

    .ant-card-actions {
      background: rgba(255, 255, 255, 0.03);
      border-top: 1px solid rgba(255, 255, 255, 0.06);
    }

    .ant-card-actions > li {
      margin: 12px 0;
    }

    .ant-card-meta-title {
      margin-bottom: 8px !important;
    }

    .ant-card-meta-description {
      color: white !important;
    }
  `,

  imageWrapper: css`
    height: 220px;
    width: 100%;
    overflow: hidden;
    background: linear-gradient(
      180deg,
      rgba(255, 255, 255, 0.04),
      rgba(0, 0, 0, 0.08)
    );
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16px;
  `,

  productImage: css`
    width: 100%;
    height: 100%;
    object-fit: contain;
    transition: transform 0.25s ease;
  `,

  productTitle: css`
    color: white;
    font-size: 16px;
    font-weight: 600;
    line-height: 1.4;
  `,

  metaContent: css`
    display: flex;
    flex-direction: column;
    gap: 6px;
  `,

  priceText: css`
    color: ${token.colorPrimary} !important;
    font-size: 18px;
    font-weight: 700;
  `,

  categoryText: css`
    color: rgba(255, 255, 255, 0.72) !important;
    font-size: 14px;
    text-transform: capitalize;
  `,

  actionBtn: css`
    &.ant-btn {
      background: transparent;
      border: none;
      color: white;
      box-shadow: none;
      font-weight: 500;
    }

    &.ant-btn:hover,
    &.ant-btn:focus {
      background: rgba(255, 255, 255, 0.06) !important;
      color: ${token.colorPrimary} !important;
      border: none !important;
      box-shadow: none !important;
    }

    &.ant-btn-dangerous:hover,
    &.ant-btn-dangerous:focus {
      color: ${token.colorError} !important;
    }
  `,

  stateWrapper: css`
    min-height: 320px;
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 16px;
  `,

  stateText: css`
    color: white !important;
    font-size: 16px;
  `,

  errorText: css`
    color: ${token.colorError} !important;
    font-size: 16px;
    font-weight: 600;
  `,
}));
```

## Inline Styling Rule

Inline styling should never be used.
All styling must be defined through the required `style.ts` pattern using `antd-style`.

## Enforcement Rule

For every new provider module, copy this exact pattern and only rename feature-specific types/actions/endpoints.
No structural deviations are allowed.
`````
