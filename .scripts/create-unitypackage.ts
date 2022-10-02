import { unitypackage } from "https://deno.land/x/unitypackage@v0.1.1/mod.ts";
import { join } from "https://deno.land/std@0.158.0/path/mod.ts";
import { Sha256 } from "https://deno.land/std@0.158.0/hash/sha256.ts";

const path = ".";
const pkg_root = "Assets/_Gizmo/UdonOTPLib";
const pkg_name = "UdonOTPLib";
const release_dir = ".release";

const pkg = new unitypackage(path);

for await (const f of traverse(path)) await pkg.addFile(f, { root: pkg_root });

const filename = `${pkg_name}${await version()}.unitypackage`;
await Deno.mkdir(release_dir).catch(() => {});
await pkg.save(join(release_dir, filename));

let checksum = new Sha256()
  .update(await Deno.readFile(join(release_dir, filename)))
  .hex();
checksum += `  ${filename}\n`;
await Deno.writeFile(
  join(release_dir, "checksums.txt"),
  new TextEncoder().encode(checksum)
);

async function* traverse(path: string): AsyncGenerator<string> {
  for await (const info of Deno.readDir(path)) {
    if (info.name[0] === ".") continue;
    if (
      info.name.length >= 5 &&
      info.name.lastIndexOf(".meta") === info.name.length - 5
    )
      continue;
    if (info.isDirectory) yield* traverse(join(path, info.name));
    if (info.isFile) yield join(path, info.name);
  }
}

async function version() {
  const cmd = Deno.run({
    cmd: "git describe --abbrev=0 --tags".split(/[ ]+/),
    stdout: "piped",
    stderr: "null",
  });
  if (!(await cmd.status()).success) return "";
  return "_" + new TextDecoder().decode(await cmd.output()).trimEnd();
}
