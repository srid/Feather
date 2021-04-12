<img width="10%" src="./feather-logo.svg">

# Feather

Feather is a work-in-progress **static site generator & previewer** with customizable pipeline and live-reload. It is currently in research & prototype phase, to explore the possibility of two related but very different use-cases:

1. Generate simple static sites
2. Generate complex static sites built off varied data that change over time (eg: Markdown files, as done by [Neuron Zettelkasten](https://neuron.zettel.page/))

Feather, taking inspiration from the likes of [Sveltekit](https://kit.svelte.dev/) (which uses [Vite](https://vitejs.dev/)), also aims to provide a live-reload approach to instantly previewing the site while its source files are being modified, but without the "taint" of bringing in JavaScript. 

## Technology

**F#** is used for a number of reasons, but primarily because the author is currently [invested in learning](https://srid.github.io/learning-fsharp/) it.

For **HTML templating** we choose Shopify's [Liquid](https://shopify.dev/docs/themes/liquid/reference) language (using [fluid](https://github.com/sebastienros/fluid)).

For building **reactive pipelines** we choose [Rx](https://dotnetfoundation.org/projects/reactive-extensions) along with possibly [Dynamic Data](https://github.com/reactivemarbles/DynamicData).

## Milestones

The first priority is to get a non-pipeline based (i.e one-off) static site generation working off Liquid files. Then, live-reload is an option (a builtin version of [LiveReloadServer](https://github.com/RickStrahl/LiveReloadServer) basically). Finally exploring the reactive pipeline approach to achieve a neuron-like use case *in a more general fashion* would be an ambitious undertaking, and the ultimate purpose of this project.

## Other considerations

- **i18n**: The generated websites should support multiple languages. The "content" would thus be specified as input for the templates, rathar than being hardcoded in the HTML tags. This would allow us to specify translated versions with their own routes (eg: `/en/slug1` vs `/fr/slug1`).
- **client-side search**: If, as detailed above, "content" is treated as *data* rather than *markup*, we can use [Elasticlunr.js](http://elasticlunr.com/) to provide a client-side search of these data documents.
- **tool compilation**: For eg, to build CSS. The initial milestones will have support for [Tailwind](https://tailwindcss.com/) CSS styling, albeit using [twind/shim](https://twind.dev/docs/handbook/getting-started/using-the-shim.html) to (lazily) begin with.

---

## Hacking

```shell
dotnet tool restore
dotnet paket restore

dotnet run -p ./src/Feather -- -w -C example
```

The above command will launch a live server that reloads the browser view as your source files change.

You can use `dotnet watch` (instead of `dotnet run`) to recompile and restart the tool on source change.

```
dotnet watch -p ./src/Feather run -- -w -C ../../example
```

## Installing and use

To build and install the release version,

```
cd ./src/Feather
dotnet publish -c Release --self-contained -r linux-x64 -o out
export FEATHER=$(pwd)/out/Feather
...
```

Use it,

```
mkdir my-site && cd my-site
mkdir templates output
echo Hello world > templates/index.liquid
$FEATHER -w .
```

**Fastest way** to get feather running is to use the released version from NuGet (which may lag behind):

```
# Copy the .fsx script to your site dir as appropriate. To test,
./example/example.fsx -C example/
```

## Status

- [x] One-off generation of `.html` from `.liquid` files (see `./example` folder)
- [x] Primitive file watcher that regenerates on source change
- [x] Tailwind CSS support in .liquid files (via `twind/shim`)
- [x] Finalize on a HTML template library ([#1](https://github.com/srid/Feather/issues/1))
- [x] Add a dev server with live-reload
- [ ] static/ files
- [ ] Deploy something useful
