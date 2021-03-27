# Feather

Feather is a work-in-progress **static site generator** with customizable pipeline and live-reload. It is currently in research & prototype phase, to explore the possibility of two related but very different use-cases:

1. Generate simple static sites
2. Generate complex static sites with data built from pipelines (eg: [Neuron Zettelkasten](https://neuron.zettel.page/))

Feather, taking inspiration from the likes of [Sveltekit](https://kit.svelte.dev/) (which uses [Vite](https://vitejs.dev/)), also aims to provide a live-reload approach to instantly previewing the site while its source files are being modified, but without the "taint" of bringing in JavaScript. 

## Technology

**F#** is used for a number of reasons, but primarily because the author is currently [invested in learning](https://srid.github.io/learning-fsharp/) it.

For **HTML templating** we choose Shopify's [Liquid](https://shopify.github.io/liquid/) language (using [DotLiquid](http://dotliquidmarkup.org/)).

For building **reactive pipelines**, we choose [Rx](https://dotnetfoundation.org/projects/reactive-extensions) along with possibly [Dynamic Data](https://dynamic-data.org/).

## Milestones

The first priority is to get a non-pipeline based (i.e one-off) static site generation working off Liquid files. Then, live-reload is an option (a builtin version of [LiveReloadServer](https://github.com/RickStrahl/LiveReloadServer) basically). Finally exploring the reactive pipeline approach to achieve a neuron-like use case *in a more general fashion* would be an ambitious undertaking, and the ultimate purpose of this project.

## Other considerations

- **i18n**: The generated websites should support multiple languages. The "content" would thus be specified as input for the templates, rathar than being hardcoded in the HTML tags. This would allow us to specify translated versions with their own routes (eg: `/en/slug1` vs `/fr/slug1`).
- **client-side search**: If, as detailed above, "content" is treated as *data* rather than *markup*, we can use [Elasticlunr.js](http://elasticlunr.com/) to provide a client-side search of these data documents.
- **tool compilation**: For eg, to build CSS. The initial milestones will have support for [Tailwind](https://tailwindcss.com/) CSS styling, albeit using [twind/shim](https://twind.dev/docs/handbook/getting-started/using-the-shim.html) to (lazily) begin with.