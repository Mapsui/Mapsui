# Logo Notes

The primary Mapsui logo is `logo.svg`.

Use the diagonal-gradient logo for new profile images, package icons, website icons,
social avatars, repository branding, and marketing images.

## Source Logo

`logo.svg` is the canonical source asset.

- The `viewBox` is `0 0 512 512`.
- The origin is in the top-left corner.
- The center of the logo is at `(256, 256)`.
- The mark is a mostly circular shape intended to suggest a capital `M`.
- The design remains left-right symmetrical.
- The background outside the mark is transparent.
- The visible mark uses a diagonal blue-to-purple gradient from top-left to bottom-right.
- The gradient transitions through `#4D84F2`, `#4F79E4`, `#526ED5`, `#5664C5`,
  `#595AB4`, `#575094`, `#4F457F`, `#473C6E`, `#40335F`, `#392C53`, and `#332548`.

## Shape Details

- The circle radius is `220`, centered at `(256, 256)`.
- A centered bottom cutout starts at the horizontal center line `y = 256`.
- That bottom cutout extends vertically downward between `x = 168` and `x = 344`.
- Its upper edge is shaped with two `40` degree diagonal cut lines that meet at the
  exact center `(256, 256)`, leaving a downward point in the middle.
- A top triangular cutout is also removed.
- That top cutout is centered horizontally and uses matching `40` degree diagonal sides.
- Its tip points downward to `(256, 128.185808)`.

## Exported Images To Keep Locally

Keep square PNG exports in `assets/generated/logo/`. Use transparent PNGs
unless a specific platform asks for an opaque background.

There are four generated families:

- `assets/generated/logo/`: transparent background with the SVG's built-in
  `36px` margin around the mark.
- `assets/generated-white-background/logo/`: white background with the SVG's built-in
  `36px` margin around the mark.
- `assets/generated-no-margin/logo/`: transparent background cropped to
  the visible logo bounds.
- `assets/generated-white-background-no-margin/logo/`: white background cropped to
  the visible logo bounds.

Use a margin version for avatars and surfaces that may apply their own circular or
rounded mask. Use a no-margin version when the image should occupy as much of the
available viewport as possible, especially for small icons, favicons, documentation
images, package icons, and designed marketing layouts.

The margin versions export the full `512x512` SVG viewBox. The no-margin versions use
the crop area `36:36:476:476`, which matches the visible circle bounds and removes the
transparent padding around the mark. The white-background versions use Inkscape's
`--export-background="#ffffff" --export-background-opacity=1` options.

| File | Size | Use this exact image for |
| --- | ---: | --- |
| `logo-16.png` | 16x16 | Browser tab favicon fallback when an `.ico` file is not used. |
| `logo-32.png` | 32x32 | Standard browser favicon in modern desktop browsers. |
| `logo-48.png` | 48x48 | Windows/browser shortcut icon and small desktop shortcut contexts. |
| `logo-64.png` | 64x64 | Small documentation, README, or admin UI logo where 32px looks too soft. |
| `logo-128.png` | 128x128 | NuGet package icon embedded in `.nupkg` files. |
| `logo-180.png` | 180x180 | Apple touch icon for "Add to Home Screen" on iPhone. |
| `logo-192.png` | 192x192 | Android/web app manifest icon for launcher and install surfaces. |
| `logo-256.png` | 256x256 | Windows desktop shortcut icon source and high-DPI docs/UI usage. |
| `logo-400.png` | 400x400 | X/Twitter profile avatar upload for the Mapsui account. |
| `logo-512.png` | 512x512 | Web app manifest large icon and general social avatar master. |
| `logo-1024.png` | 1024x1024 | High-resolution source export for stores, press kits, and future resizing. |

## Platform Usage

Use these specific placements when updating Mapsui branding:

- X/Twitter account profile image: upload `logo-400.png`.
- X/Twitter posts that need a standalone logo image: use `logo-512.png`
  or `logo-1024.png`, depending on the composition size.
- GitHub organization avatar: upload `logo-1024.png`; GitHub will resize it.
- GitHub repository README logo image: use the SVG directly when possible, otherwise use
  `logo-512.png`.
- DocFX API documentation logo: keep `docs/api/images/logo.svg` in sync with
  `assets/logo.svg` because `docs/api/metadata.json` references that docs-local path.
- MkDocs general documentation logo and favicon: keep
  `docs/general/markdown/images/logo.svg` and
  `docs/general/markdown/images/favicon.ico` in sync with the canonical assets because
  `docs/general/mkdocs.yml` references those docs-local paths.
- NuGet package icon: include `logo-128.png` in the package and reference it
  with the package `icon` metadata.
- Website favicon: use `logo-32.png` and optionally package `16`, `32`, and
  `48` pixel versions into a single `.ico`.
- Apple home-screen bookmark icon: use `logo-180.png`.
- PWA or web app manifest icon set: include `logo-192.png` and
  `logo-512.png`.
- Presentation slide logo: use the SVG for vector output, or `logo-1024.png`
  when the presentation tool handles PNGs more reliably.
- Press kit or sponsor logo download: provide the SVG plus `logo-1024.png`.

## Regenerate Exports

Use `generate-logo-assets.ps1` after changing `logo.svg`. It rebuilds every PNG size
for all four generated families.

The script requires Inkscape to be installed locally and available on `PATH` as
`inkscape`.

```powershell
.\assets\generate-logo-assets.ps1
```

If Inkscape is installed but not on `PATH`, pass the executable path explicitly:

```powershell
.\assets\generate-logo-assets.ps1 -Inkscape "C:\Program Files\Inkscape\bin\inkscape.com"
```

The script generates these four export types:

- Transparent background with margin.
- White background with margin.
- Transparent background with no margin.
- White background with no margin.

## Notes Behind The Sizes

- NuGet package icons should use a PNG or JPEG image, with `128x128` recommended and
  a 1 MB package icon file limit.
- GitHub organization avatars are uploaded through the organization profile settings;
  use `1024x1024` locally so GitHub can resize from a clean square source.
- Web app manifests should include multiple icon sizes so browsers can pick the best
  available image for each install or launcher context.
- X/Twitter profile images are square uploads; keep `400x400` as the direct avatar file
  and `512x512` or `1024x1024` as safer master files for reuse.
