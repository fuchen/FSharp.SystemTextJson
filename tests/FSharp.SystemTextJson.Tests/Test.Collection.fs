module Tests.Collection

open System.Collections.Generic
open System.Text.Json.Serialization
open System.Text.Json
open Xunit
open FsCheck
open FsCheck.Xunit

let options = JsonSerializerOptions()
options.Converters.Add(JsonFSharpConverter())

[<Property>]
let ``deserialize list of ints`` (l: list<int>) =
    let ser = "[" + String.concat "," (List.map string l) + "]"
    let actual = JsonSerializer.Deserialize<list<int>>(ser, options)
    Assert.Equal<list<int>>(l, actual)

[<Property>]
let ``serialize list of ints`` (l: list<int>) =
    let expected = "[" + String.concat "," (List.map string l) + "]"
    let actual = JsonSerializer.Serialize(l, options)
    Assert.Equal(expected, actual)

[<Property>]
let ``deserialize set of ints`` (s: Set<int>) =
    let ser = "[" + String.concat "," (Seq.map string s) + "]"
    let actual = JsonSerializer.Deserialize<Set<int>>(ser, options)
    Assert.Equal<Set<int>>(s, actual)

[<Property>]
let ``serialize set of ints`` (s: Set<int>) =
    let expected = "[" + String.concat "," (Seq.map string s) + "]"
    let actual = JsonSerializer.Serialize(s, options)
    Assert.Equal(expected, actual)

let serKV1 (KeyValue (k: string, v: int)) =
    JsonSerializer.Serialize(k) + ":" + JsonSerializer.Serialize(v)

[<Property>]
let ``deserialize string-keyed map`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add k v m)
    let ser = "{" + String.concat "," (Seq.map serKV1 m) + "}"
    let actual = JsonSerializer.Deserialize<Map<string, int>>(ser, options)
    Assert.Equal<Map<string, int>>(m, actual)

[<Property>]
let ``serialize string-keyed map`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add k v m)
    let expected = "{" + String.concat "," (Seq.map serKV1 m) + "}"
    let actual = JsonSerializer.Serialize(m, options)
    Assert.Equal(expected, actual)

type UserId = UserId of string

let serKV1_1 (KeyValue (UserId k, v: int)) =
    JsonSerializer.Serialize(k) + ":" + JsonSerializer.Serialize(v)

[<Property>]
let ``deserialize newtype-string-keyed map`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add (UserId k) v m)
    let ser = "{" + String.concat "," (Seq.map serKV1_1 m) + "}"
    let actual = JsonSerializer.Deserialize<Map<UserId, int>>(ser, options)
    Assert.Equal<Map<UserId, int>>(m, actual)

[<Property>]
let ``serialize newtype-string-keyed map`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add (UserId k) v m)
    let expected = "{" + String.concat "," (Seq.map serKV1_1 m) + "}"
    let actual = JsonSerializer.Serialize(m, options)
    Assert.Equal(expected, actual)

[<Struct>]
type SUserId = SUserId of string

let serKV1_2 (KeyValue (SUserId k, v: int)) =
    JsonSerializer.Serialize(k) + ":" + JsonSerializer.Serialize(v)

[<Property>]
let ``deserialize struct-newtype-string-keyed map`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add (SUserId k) v m)
    let ser = "{" + String.concat "," (Seq.map serKV1_2 m) + "}"
    let actual = JsonSerializer.Deserialize<Map<SUserId, int>>(ser, options)
    Assert.Equal<Map<SUserId, int>>(m, actual)

[<Property>]
let ``serialize struct-newtype-string-keyed map`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add (SUserId k) v m)
    let expected = "{" + String.concat "," (Seq.map serKV1_2 m) + "}"
    let actual = JsonSerializer.Serialize(m, options)
    Assert.Equal(expected, actual)

let keyPolicyOptions = JsonSerializerOptions(DictionaryKeyPolicy = JsonNamingPolicy.CamelCase)
keyPolicyOptions.Converters.Add(JsonFSharpConverter())

[<Property>]
let ``deserialize string-keyed map with key policy`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add k v m)
    let ser = "{" + String.concat "," (Seq.map serKV1 m) + "}"
    let actual = JsonSerializer.Deserialize<Map<string, int>>(ser, keyPolicyOptions)
    Assert.Equal<Map<string, int>>(m, actual)

[<Property>]
let ``serialize string-keyed map with key policy`` (m: Map<NonNull<string>, int>) =
    let m = (Map.empty, m) ||> Map.fold (fun m (NonNull k) v -> Map.add k v m)
    let ccm = m |> Seq.map (fun (KeyValue(k, v)) -> KeyValuePair(JsonNamingPolicy.CamelCase.ConvertName k, v))
    let expected = "{" + String.concat "," (Seq.map serKV1 ccm) + "}"
    let actual = JsonSerializer.Serialize(m, keyPolicyOptions)
    Assert.Equal(expected, actual)

let serKV2 (KeyValue (k: int, v: string)) =
    "[" + JsonSerializer.Serialize(k) + "," + JsonSerializer.Serialize(v) + "]"

[<Property>]
let ``deserialize int-keyed map`` (m: Map<int, string>) =
    let ser = "[" + String.concat "," (Seq.map serKV2 m) + "]"
    let actual = JsonSerializer.Deserialize<Map<int, string>>(ser, options)
    Assert.Equal<Map<int, string>>(m, actual)

[<Property>]
let ``serialize int-keyed map`` (m: Map<int, string>) =
    let expected = "[" + String.concat "," (Seq.map serKV2 m) + "]"
    let actual = JsonSerializer.Serialize(m, options)
    Assert.Equal(expected, actual)

[<Property>]
let ``deserialize 2-tuple`` ((a, b as t): int * string) =
    let ser = sprintf "[%i,%s]" a (JsonSerializer.Serialize b)
    let actual = JsonSerializer.Deserialize<int * string>(ser, options)
    Assert.Equal(t, actual)

[<Property>]
let ``serialize 2-tuple`` ((a, b as t): int * string) =
    let expected = sprintf "[%i,%s]" a (JsonSerializer.Serialize b)
    let actual = JsonSerializer.Serialize(t, options)
    Assert.Equal(expected, actual)

[<Property>]
let ``deserialize 8-tuple`` ((a, b, c, d, e, f, g, h as t): int * int * int * int * int * int * int * int) =
    let ser = sprintf "[%i,%i,%i,%i,%i,%i,%i,%i]" a b c d e f g h
    let actual = JsonSerializer.Deserialize<int * int * int * int * int * int * int * int>(ser, options)
    Assert.Equal(t, actual)

[<Property>]
let ``serialize 8-tuple`` ((a, b, c, d, e, f, g, h as t): int * int * int * int * int * int * int * int) =
    let expected = sprintf "[%i,%i,%i,%i,%i,%i,%i,%i]" a b c d e f g h
    let actual = JsonSerializer.Serialize(t, options)
    Assert.Equal(expected, actual)

[<Property>]
let ``deserialize struct 2-tuple`` ((a, b as t): struct (int * string)) =
    let ser = sprintf "[%i,%s]" a (JsonSerializer.Serialize b)
    let actual = JsonSerializer.Deserialize<struct (int * string)>(ser, options)
    Assert.Equal(t, actual)

[<Property>]
let ``serialize struct 2-tuple`` ((a, b as t): struct (int * string)) =
    let expected = sprintf "[%i,%s]" a (JsonSerializer.Serialize b)
    let actual = JsonSerializer.Serialize(t, options)
    Assert.Equal(expected, actual)

[<Property>]
let ``deserialize struct 8-tuple`` ((a, b, c, d, e, f, g, h as t): struct (int * int * int * int * int * int * int * int)) =
    let ser = sprintf "[%i,%i,%i,%i,%i,%i,%i,%i]" a b c d e f g h
    let actual = JsonSerializer.Deserialize<struct (int * int * int * int * int * int * int * int)>(ser, options)
    Assert.Equal(t, actual)

[<Property>]
let ``serialize struct 8-tuple`` ((a, b, c, d, e, f, g, h as t): struct (int * int * int * int * int * int * int * int)) =
    let expected = sprintf "[%i,%i,%i,%i,%i,%i,%i,%i]" a b c d e f g h
    let actual = JsonSerializer.Serialize(t, options)
    Assert.Equal(expected, actual)
