import type { Cikk } from "./Cikk"

export type Szerzo = {
    id: number,
    name: string,
    article?: Cikk[]
}