# v0.5.0-beta.1

* __[Breaking]__ Changed namespace for all existing classes from `Id3` to `Id3.Files`.

## FileNamer
* __[Breaking]__ Removed the `Rename` methods on `FileNamer`. The class will now only provide suggestions.
* __[Breaking]__ Added a named parameter `fileMask` to the `GetSuggestions` method, instead of hardcoding the file mask as `*.mp3`.
* Allowing multiple patterns to be specified for `FileNamer`, ordered according to priority. Added additional contructor overloads to support this feature.
* __[Breaking]__ Remove the default value for the `pattern` parameter in the original `FileNamer` constructor, which accepted a string. The consumer must now provide a pattern.
* __[Breaking]__ Allowing only a predefined set of placeholders that can be used in the file naming patterns, instead of any available frame property.