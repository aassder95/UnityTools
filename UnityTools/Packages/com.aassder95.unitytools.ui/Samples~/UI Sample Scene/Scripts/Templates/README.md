# Sample Template Guide

## Files
- `TemplateSampleModel.cs`
- `TemplateSampleView.cs`
- `TemplateSamplePresenter.cs`
- `TemplateSampleModule.cs`

## How To Use
1. Copy the `Templates` folder files and rename `TemplateSample*` to your sample name.
2. Change `TemplateSampleModule._moduleKey` to a unique key (for example `Quest`).
3. Attach your module component to a scene object.
4. Assign the module to `UiManager._sampleModules`.
5. Set `UiManager._entryModuleKey`:
   - `All` to run all modules
   - your module key to run only one sample
