# USCF Tournament & Game Manager

A Windows Forms desktop application that replaces the browser-based Tampermonkey script for managing USCF tournament results and game links.

## Features

### Core Functionality
- ✅ **Direct USCF API Integration** - Uses official USCF Ratings API (no scraping required)
- ✅ **Game-by-Game Display** - Shows detailed match results with opponents, ratings, and outcomes
- ✅ **Game Link Management** - Click to add ChessBase/Lichess/Chess.com URLs for tournaments
- ✅ **S3 Sync** - Stores game links in JSON on S3 for multi-computer access
- ✅ **HTML Generation** - Creates same format as old Tampermonkey system
- ✅ **Live Preview** - See tournament data exactly as it will appear on the web

### UI Features
- 📊 **Statistics Dashboard** - Tournament count, W-L-D record, linked games percentage
- 🎨 **Color Coding** - Green rows for games with links, easy visual scanning
- 🔗 **Quick Actions**:
  - Click "Games" column to add/open game links
  - Double-click any row to open USCF tournament page
  - Multi-select rows for bulk link assignment
- ⚙️ **AWS Settings Dialog** - Secure credential storage

## Getting Started

### Prerequisites
- Windows 10/11
- .NET 9.0 Runtime
- AWS credentials with S3 access to `uscf-results` bucket

### First Run Setup

1. **Build the Application**
   ```bash
   cd "C:\data\chess\apps\USCF Game List\USCF Game List"
   dotnet build
   ```

2. **Run the Application**
   ```bash
   dotnet run
   ```
   Or open the solution in Visual Studio and press F5.

3. **Configure AWS Settings**
   - Click "Settings" button
   - Enter your AWS Access Key and Secret Key
   - Verify S3 Bucket is set to `uscf-results`
   - Set Region to `us-west-2`
   - Click "Save"

4. **Load Your Data**
   - The USCF ID field defaults to `30635618`
   - Click "Refresh Events" to fetch from USCF API
   - Wait for data to load (may take 30-60 seconds for full history)

## How to Use

### Fetching Tournament Data
1. Enter your USCF ID (or leave default)
2. Click **Refresh Events**
3. The app will:
   - Fetch all games from USCF API
   - Fetch section data (for rating changes)
   - Merge data with existing game links from S3
   - Display in grid

### Adding Game Links
**Single Tournament:**
1. Find the tournament row with no link
2. Click the empty cell in the "Games" column
3. Paste your ChessBase/Lichess URL
4. Click "Save"

**Multiple Tournaments:**
1. Hold Ctrl and click multiple rows
2. Right-click → "Add Game Link" (future feature)
3. Or click the "Games" column on any selected row
4. Enter URL and it applies to all selected

**Auto-Clipboard Detection:**
- If you have a URL in your clipboard, the dialog will auto-populate it

### Uploading to S3
1. Click **Upload to S3**
2. Review the confirmation:
   - `game_links.json` - Your tournament→URL mappings
   - `index_new.html` - The generated HTML file
3. Click "Yes" to upload
4. Check the status bar for progress

### Viewing Results
- **Web Preview:** http://uscf-results.s3-website-us-west-2.amazonaws.com/index_new.html
- **Old System (still works):** http://uscf-results.s3-website-us-west-2.amazonaws.com/index.html

## Data Storage

### Local
- **Settings:** `%LocalAppData%\USCFGameList\settings.json`
  - AWS credentials (not encrypted - use at your own risk)
  - S3 bucket and region config

### S3 Bucket: `uscf-results`
- **game_links.json** - Tournament ID→Game URL mappings (new system)
- **game data.txt** - CSV format (old system, read-only)
- **index_new.html** - Generated tournament results (new system)
- **index.html** - Old Tampermonkey output (unchanged)

## File Structure

```
C:\data\chess\apps\USCF Game List\
├── USCF Game List\
│   ├── Models\
│   │   └── USCFModels.cs          # API response & display models
│   ├── Services\
│   │   ├── USCFApiClient.cs       # USCF API integration
│   │   ├── S3Service.cs           # AWS S3 operations
│   │   └── HtmlGenerator.cs       # HTML table generation
│   ├── Dialogs\
│   │   ├── AddGameLinkDialog.cs   # Add/edit game URLs
│   │   └── SettingsDialog.cs      # AWS settings
│   ├── Form1.cs                   # Main application logic
│   ├── Form1.Designer.cs          # UI layout
│   └── Program.cs                 # Entry point
└── README.md                      # This file
```

## API Details

### USCF Ratings API Endpoints Used
- `GET /api/v1/members/{id}/games` - Individual game records
- `GET /api/v1/members/{id}/sections` - Section-level data (for player ratings)

### Data Merging
The app merges data from two endpoints:
1. **Games endpoint** → Opponent names, ratings, results, rounds
2. **Sections endpoint** → Player's pre/post ratings per tournament

This provides complete information matching the old Tampermonkey output.

## Migration from Old System

### Side-by-Side Testing
The new system writes to `index_new.html` so you can:
1. Keep using Tampermonkey script (writes to `index.html`)
2. Test new WinForms app (writes to `index_new.html`)
3. Compare outputs side-by-side
4. When confident, switch to `index.html`

### Game Links Compatibility
- **Old system:** Uses `game data.txt` (CSV: tournament name, URL)
- **New system:** Uses `game_links.json` (JSON: event ID → URL)
- Both files coexist on S3
- New system doesn't modify old `game data.txt`

### To Fully Migrate
1. Test the new app thoroughly
2. Update Form1.cs line 312: Change `"index_new.html"` to `"index.html"`
3. Rebuild and deploy
4. Retire Tampermonkey script

## Troubleshooting

### "S3: Not configured"
- Click Settings and enter AWS credentials
- Verify bucket name is `uscf-results`

### "Failed to load game links"
- Check AWS credentials are valid
- Verify S3 bucket exists and you have read permissions
- `game_links.json` may not exist yet (that's okay on first run)

### No games showing after refresh
- Verify USCF ID is correct
- Check internet connection
- USCF API may be down (rare)
- Try again in a few minutes

### Build errors
- Ensure .NET 9.0 SDK is installed
- Run `dotnet restore` before building
- Close Visual Studio and reopen solution

### Nullable warnings during build
- These are safe to ignore
- They're related to WinForms designer-generated code

## Future Enhancements

Potential features for later:
- [ ] Backup/restore game links to local disk
- [ ] Import/export functionality
- [ ] Preview HTML before upload
- [ ] Multi-player support (track multiple USCF IDs)
- [ ] Command-line mode for automation
- [ ] HTML template customization
- [ ] Bulk operations (delete links, copy URLs)

## Support

Created by: Claude (AI Assistant)
For questions or issues: Contact the repository owner

## License

Personal use project - no formal license
