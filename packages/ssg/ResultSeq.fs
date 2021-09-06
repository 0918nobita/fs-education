module ResultSeq

let resultSequence2 (resSeq: seq<Result<'a, 'b>>) : Result<seq<'a>, seq<'b>> =
    resSeq
    |> Seq.fold
        (fun s a ->
            match s, a with
            | Ok arr, Ok v -> Ok(Seq.append arr (Seq.singleton v))
            | Ok _, Error e -> Error(Seq.singleton e)
            | Error es, Ok _ -> Error es
            | Error es, Error e -> Error(Seq.append es (Seq.singleton e)))
        (Ok Seq.empty)
