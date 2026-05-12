# Why Stratum Matters

*A short manifesto on why the modern web stack is broken — and why now is the moment
to do something about it.*

---

## The Web Is Broken (And We Just Got Used to It)

In the early days of personal computing, building a software application was relatively straightforward. You picked a language, wrote your logic, and the result ran on the screen. Visual Basic, Delphi, early WinForms — you dragged a button onto a canvas, double-clicked it, wrote a few lines of code, hit F5, and had a working app. The feedback loop was tight. The mental model was simple. Computers boomed.

Then the web happened.

The web was designed to display *documents* — research papers, linked pages of text. It was never designed for *applications*. But over thirty years, we gradually bent it into an application platform through an ever-growing tower of workarounds: HTML for structure, CSS for styling, JavaScript for behavior, then frameworks on top of JavaScript, then build tools on top of frameworks, then package managers to manage the build tools. Today, a "simple" web app routinely requires a developer to understand five or more distinct languages and toolchains simultaneously — just to put a button on a screen.

This is not progress. It is accumulated technical debt, normalized.

---

## What Stratum Is

Stratum is a framework that lets you build real, interactive web applications using **one language** — the language you already know, whether that's C#, Rust, Go, or another compiled language — and **zero web cruft**.

No HTML. No CSS. No JavaScript you write yourself. No build pipelines to configure.

You write code. You compile. You get a working app in any modern browser.

The browser is treated for what it actually is: a runtime capable of executing compiled code at near-native speed, with a pixel buffer to draw on. Stratum draws directly to that pixel buffer — the HTML `<canvas>` element — and builds everything else (buttons, text boxes, layout, events) from scratch on top of it.

This is not a workaround. It is a clean break.

---

## The Key Insight: Kill the DOM

The Document Object Model — the DOM — is the data structure browsers use to represent a web page. It was designed for documents. Every major web framework of the last twenty years (jQuery, Angular, React, Vue, Svelte) has been, at its core, a sophisticated attempt to make the DOM behave like an application runtime. They have all partially succeeded and all fundamentally struggled, because the mismatch is architectural.

Stratum does not fight the DOM. It ignores it entirely.

The application gets one canvas element. Everything rendered inside that canvas is controlled entirely by compiled application code. The browser is reduced to its role as a host — providing the window, the input events, and the pixel buffer. Nothing more.

This returns development to the model that made computing productive in the first place: you write code that directly controls what appears on screen, with no intermediary document model in the way.

---

## Why This Matters Right Now: The WASM Moment

WebAssembly (WASM) is the technology that makes this possible. It is a binary instruction format that runs in every modern browser at near-native speed. It means compiled C#, Rust, or Go code can run in the browser — not transpiled, not interpreted, but compiled and executed directly.

WASM has existed for several years, but its implications have not yet been fully acted on. The web development community has largely used it as an optimization tool within the existing paradigm rather than as a foundation for a new one.

Stratum is a bet that the right response to WASM is not "make React faster" — it is "replace the entire web stack with something that was designed for applications from the start."

---

## Why This Matters for AI Agents

AI coding assistants — GitHub Copilot, Claude, ChatGPT, and future autonomous agents — are increasingly writing software. The multi-language web stack creates a specific and severe problem for these systems:

- Generating correct HTML, CSS, and JavaScript *simultaneously*, with all three layers interacting correctly, is one of the hardest tasks for an LLM.
- A mistake in CSS breaks the layout. A mistake in JS breaks the behavior. The error is often invisible until runtime, and the source is often in a different file from the symptom.
- The more languages and layers involved, the larger the context required, and the more opportunities for the model to generate plausible-but-wrong output.

Stratum reduces the problem to a single language and a single API. An AI can reason about a C# method that draws a button and handles a click without switching mental models mid-generation. The output is a single, verifiable source file.

There is also a DSL (domain-specific language) component — a concise, text-based syntax for describing UIs — that is specifically designed for AI generation. Rather than writing full code, an AI can emit a compact description of the interface and have the framework parse and render it. This is the same principle that made Mermaid diagrams successful: not the prettiest output, but the most frictionless path from intent to result.

---

## The Honest Tradeoffs

Stratum is not for every use case. It trades away:

- **Native browser accessibility** — screen readers, tab order, and autofill don't work for free inside a canvas. These can be rebuilt, but they require explicit effort.
- **SEO** — canvas content is not indexable. For public-facing websites, this matters.
- **Text selection and browser find-in-page** — standard browser features don't reach inside a canvas.

It is explicitly designed for **applications**, not websites. Internal tools, dashboards, data-entry forms, business apps, and developer tooling — places where the people using the app are known, the environment is controlled, and logic matters more than discoverability.

For those use cases, the tradeoffs are acceptable, and the gains are substantial.

---

## The Analogy That Fits

Think of Mermaid diagrams. Mermaid is not the most powerful diagramming tool. It doesn't produce the most beautiful output. But it lets you write a diagram as plain text, check it into source control, generate it from code, and hand it to an AI to produce or modify. It occupies a specific niche — frictionless, text-first, tool-friendly diagrams — and it wins that niche decisively.

Stratum aims for the same position in application development. Not the most powerful UI framework. Not the most beautiful output. But the most frictionless path from intent to running application — for humans, and especially for AI agents.

---

## What Success Looks Like

A developer (or an AI agent) writes a single source file in a language they already know. They run one command. A `.wasm` file is produced. They drop it next to a four-line HTML loader. The app runs in any browser, with no npm, no webpack, no CSS files, no JavaScript written by hand.

That is the prototype goal. Everything else can be built from there.

---

*Stratum is a prototype and a proof of concept. It is a bet that the right response to thirty years of web complexity is not another abstraction layer — it is a clean break, grounded in a runtime (WebAssembly) that finally makes that break possible.*
