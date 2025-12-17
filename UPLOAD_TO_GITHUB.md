# How to Upload to GitHub

Since GitHub CLI is not installed, follow these steps to upload the repository:

## Option 1: Using GitHub Website

1. Go to https://github.com/new
2. Create a new repository named `shape-of-dreams-mods`
3. Make it **Public**
4. **DO NOT** initialize with README, .gitignore, or license (we already have these)
5. Click "Create repository"
6. Copy the repository URL (e.g., `https://github.com/YOUR_USERNAME/shape-of-dreams-mods.git`)
7. Open PowerShell in this directory and run:

```powershell
cd "c:\Users\Juliu\Desktop\shape-of-dreams-mods"
git remote add origin https://github.com/YOUR_USERNAME/shape-of-dreams-mods.git
git branch -M main
git push -u origin main
```

## Option 2: Using GitHub Desktop

1. Download and install GitHub Desktop: https://desktop.github.com/
2. Open GitHub Desktop
3. Click "File" → "Add Local Repository"
4. Browse to `c:\Users\Juliu\Desktop\shape-of-dreams-mods`
5. Click "Publish repository"
6. Choose name: `shape-of-dreams-mods`
7. Make it **Public**
8. Click "Publish Repository"

## What's Included

✅ All C# source files for both mods
✅ All localization files (i18n/*.json)
✅ Visual Studio solution file (.sln)
✅ Project files (.csproj) for each mod
✅ README files with documentation
✅ .gitignore to exclude build artifacts

❌ Build scripts (.bat files) - excluded as they're environment-specific
❌ Images - excluded due to size (distribute separately)
❌ Binary files (.dll) - excluded, users should build from source

## Repository Structure

```
shape-of-dreams-mods/
├── README.md                          # Main repository documentation
├── ShapeOfDreamsMods.sln             # Visual Studio solution
├── .gitignore                         # Git ignore rules
│
├── RPGItemsMod/
│   ├── README.md                      # RPGItemsMod documentation
│   ├── RPGItemsMod.csproj            # Project file
│   ├── *.cs                           # 32 source files
│   └── i18n/                          # 13 language files
│
└── InfiniteDungeonMod/
    ├── README.md                      # InfiniteDungeonMod documentation
    ├── InfiniteDungeonMod.csproj     # Project file
    ├── *.cs                           # 9 source files
    └── i18n/                          # 13 language files
```

## After Upload

Once uploaded, users can:
- Clone the repository
- View source code
- Build the mods themselves
- Contribute improvements
- Report issues

The repository is ready to push!
