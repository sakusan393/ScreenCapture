# Code signing policy

ScreenCapture release binaries are built from this public repository by GitHub
Actions on GitHub-hosted Windows runners. An initial unsigned release may be
published while the project is applying for SignPath Foundation approval; such
a release is identified as unsigned in its release notes.

Once the project is accepted, signed releases use the following service:

> Free code signing provided by [SignPath.io](https://signpath.io/), certificate
> by [SignPath Foundation](https://signpath.org/).

## Team roles

- Committer and reviewer: [393](https://393.bz/) ([sakusan393](https://github.com/sakusan393))
- Approver: [393](https://393.bz/) ([sakusan393](https://github.com/sakusan393))

The approver checks that the release comes from the documented GitHub Actions
workflow and corresponds to an intended version before approving each signing
request.

## Privacy policy

ScreenCapture does not transfer information to other networked systems. See the
complete [privacy policy](PRIVACY.md) for its local clipboard, file, settings,
and single-instance operations.

## Build and release policy

- Release artifacts must be produced by a workflow committed to the default
  branch and executed on a GitHub-hosted Windows runner.
- The release tag, project version, and executable version must agree.
- The unsigned artifact must be stored as a GitHub Actions artifact before a
  signing request is submitted.
- Every signing request requires manual approval by the approver listed above.
- Only the signed artifact returned by SignPath may be attached to a release
  described as signed.
