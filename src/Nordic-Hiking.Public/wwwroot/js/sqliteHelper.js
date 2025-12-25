window.sqliteHelper = {
    db: null,
    SQL: null,

    async initialize(dbPath) {
        try {
            // Initialize SQL.js
            this.SQL = await initSqlJs({
                locateFile: file => `https://cdnjs.cloudflare.com/ajax/libs/sql.js/1.8.0/${file}`
            });

            // Fetch the database file
            const response = await fetch(dbPath);
            const arrayBuffer = await response.arrayBuffer();
            const uint8Array = new Uint8Array(arrayBuffer);

            // Open the database
            this.db = new this.SQL.Database(uint8Array);

            console.log('SQLite database loaded successfully');
            return true;
        } catch (error) {
            console.error('Error loading database:', error);
            return false;
        }
    },

    async queryLocations() {
        if (!this.db) {
            console.error('Database not initialized');
            return [];
        }

        try {
            const query = `
                SELECT
                    l.Id,
                    l.PlaceName,
                    l.Region,
                    l.Country,
                    l.Latitude,
                    l.Longitude,
                    l.Summary,
                    l.Difficulty,
                    l.Duration,
                    l.Confidence,
                    v.Title as VideoTitle,
                    'https://www.youtube.com/watch?v=' || v.YoutubeVideoId as VideoUrl,
                    v.ThumbnailUrl as VideoThumbnail,
                    v.PublishedAt,
                    v.VideoTalkSummary,
                    c.Name as ChannelName
                FROM HikeLocations l
                INNER JOIN Videos v ON l.VideoId = v.Id
                INNER JOIN Channels c ON v.ChannelId = c.Id
                WHERE v.ShowOnMap = 1
            `;

            const result = this.db.exec(query);

            if (result.length === 0) {
                return [];
            }

            const columns = result[0].columns;
            const values = result[0].values;

            // Convert to array of objects
            const locations = values.map(row => {
                const obj = {};
                columns.forEach((col, index) => {
                    obj[col] = row[index];
                });
                return obj;
            });

            console.log(`Loaded ${locations.length} locations from database`);
            return locations;
        } catch (error) {
            console.error('Error querying database:', error);
            return [];
        }
    },

    close() {
        if (this.db) {
            this.db.close();
            this.db = null;
        }
    }
};
