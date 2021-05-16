# UnPak

Extensible and configurable support for reading, packing, and unpacking UE4 `pak` files.

## What is it?

A simple (though extremely WIP) library and accompanying command-line app for packing and unpacking `pak` files created for/from UE4. Particularly focussed on being highly customizable, and useful for embedding in other projects or tools.

## What isn't it?

A replacement for `u4pak` or `UnrealPak`. Both `u4pak` and `UnrealPak` are more capable, more tested and designed for more scenarios. 

In particular, UnPak is first-and-foremost a library and (at this early stage) the `upk` CLI app is highly untested and likely to change dramatically.

## Usage

Documentation is not currently available while the library is still being built up, but is designed to be usable either directly or through DI.

### Projects

There are three projects making up the UnPak suite:

- **`UnPak.Core`**: The real heart of UnPak, this is the most basic no-dependency package.
- **`UnPak`**: A highly-opinionated library designed to be easier and more predictable to use, but at the cost of size and dependencies
- **`UnPak.Console`**: The `upk` CLI app powered by `UnPak.Core`.