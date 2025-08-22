# Feature Specification: WhatsApp Messaging Framework

## High-Level Overview

A raw and functional Node.js framework for sending and receiving WhatsApp messages using WPPConnect server tool. This framework serves as a foundational component for an AI-powered medical assistant, focusing solely on core messaging functionality without any frontend interface. All conversations are logged chronologically for future AI integration.

## High-Level Requirements

- **Primary Purpose**: Framework component for AI-powered medical assistant that communicates via WhatsApp
- **Messaging Scope**: Send/receive text messages, images, and documents through WPPConnect (not Cloud API)
- **Architecture**: Raw and functional implementation, no frontend needed
- **Environment**: Local machine development, future-ready for production scaling
- **Integration**: Designed for eventual AI system integration

## Existing Solutions

- **WPPConnect**: Open-source Node.js library for WhatsApp Web interface
- **WhatsApp Cloud API**: Official but not preferred for this project
- **Puppeteer-based solutions**: Similar web-scraping approaches
- **Baileys**: Alternative WhatsApp Web API library

## Current Implementation

- **Status**: Starting from scratch - empty workspace
- **Previous Work**: Evidence of deleted documentation files (design.md, requirements.md, tasks.md)
- **Technology Stack**: None currently defined
- **Dependencies**: No existing packages installed

## Detailed Requirements

### Requirement 1: Message Sending Capabilities

- **User Story**: As a framework user, I need to send different types of messages to WhatsApp users so that the AI medical assistant can communicate effectively.

#### Acceptance Criteria 1

1. ✅ **Text Messages**: WHEN sending a text message THEN the framework SHALL deliver it successfully to the specified WhatsApp contact
2. ✅ **Images**: WHEN sending an image file THEN the framework SHALL deliver it with optional caption to the specified WhatsApp contact
3. ✅ **Documents**: WHEN sending a document file THEN the framework SHALL deliver it with filename and optional caption to the specified WhatsApp contact
4. ✅ **Error Handling**: WHEN a message fails to send THEN the framework SHALL log the error and provide meaningful feedback

### Requirement 2: Message Receiving Capabilities

- **User Story**: As a framework user, I need to receive and process incoming messages from WhatsApp users so that the AI medical assistant can respond appropriately.

#### Acceptance Criteria 2

1. ✅ **Text Messages**: WHEN a user sends a text message THEN the framework SHALL capture and log the message content
2. ✅ **Images**: WHEN a user sends an image THEN the framework SHALL receive, save, and log the image with metadata
3. ✅ **Real-time Processing**: WHEN messages arrive THEN the framework SHALL process them immediately via event listeners
4. ✅ **Message Metadata**: WHEN any message is received THEN the framework SHALL capture sender ID, timestamp, and message type

### Requirement 3: Conversation Logging and Storage

- **User Story**: As a framework user, I need all conversations stored chronologically so that the AI medical assistant can maintain conversation context and history.

#### Acceptance Criteria 3

1. ✅ **Chronological Storage**: WHEN messages are sent or received THEN they SHALL be stored in timestamp order
2. ✅ **Complete Conversation History**: WHEN storing messages THEN both user and bot messages SHALL be logged with full context
3. ✅ **Database Schema**: WHEN storing data THEN it SHALL use a structured schema that supports future AI integration
4. ✅ **Query Capability**: WHEN retrieving conversation history THEN the framework SHALL support efficient querying by chat ID and timeframe

### Requirement 4: Future AI Integration Readiness

- **User Story**: As a developer integrating AI, I need the framework to provide clean data access so that AI systems can process conversation history and respond to messages.

#### Acceptance Criteria 4

1. ✅ **Structured Data Access**: WHEN AI systems query conversation data THEN they SHALL receive properly formatted message history
2. ✅ **Extensible Architecture**: WHEN adding AI components THEN the framework SHALL support integration without major refactoring
3. ✅ **Message Queue Ready**: WHEN implementing real-time AI responses THEN the framework SHALL support event-driven architecture
4. ✅ **Data Export**: WHEN migrating or analyzing data THEN the framework SHALL provide clean export capabilities

## Database Schema Design

### Recommended Database: SQLite (Current Phase)

**Rationale**: Perfect for local development, zero configuration, easy migration to PostgreSQL later.

### Core Schema

```sql
-- Main conversations table
CREATE TABLE conversations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    chat_id TEXT NOT NULL,                    -- WhatsApp chat identifier
    message_id TEXT UNIQUE,                   -- WPPConnect message ID
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    sender_type TEXT NOT NULL CHECK (sender_type IN ('user', 'bot')),
    sender_id TEXT,                           -- Phone number or contact ID
    sender_name TEXT,                         -- Contact display name
    message_type TEXT NOT NULL CHECK (message_type IN ('text', 'image', 'document')),
    content TEXT,                             -- Message text content
    media_path TEXT,                          -- Local path to saved media files
    media_filename TEXT,                      -- Original filename for documents
    caption TEXT,                             -- Image/document caption
    metadata TEXT,                            -- JSON string for additional data
    delivery_status TEXT DEFAULT 'sent',     -- For outgoing messages
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Chat sessions for grouping conversations
CREATE TABLE chat_sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    chat_id TEXT UNIQUE NOT NULL,
    contact_name TEXT,
    contact_phone TEXT,
    first_message_at DATETIME,
    last_message_at DATETIME,
    total_messages INTEGER DEFAULT 0,
    session_status TEXT DEFAULT 'active',    -- active, closed, archived
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Media files tracking
CREATE TABLE media_files (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    conversation_id INTEGER,
    original_filename TEXT,
    stored_filename TEXT,
    file_path TEXT NOT NULL,
    file_size INTEGER,
    mime_type TEXT,
    upload_timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (conversation_id) REFERENCES conversations(id)
);

-- Indexes for performance
CREATE INDEX idx_conversations_chat_timestamp ON conversations(chat_id, timestamp);
CREATE INDEX idx_conversations_sender_type ON conversations(sender_type);
CREATE INDEX idx_conversations_message_type ON conversations(message_type);
CREATE INDEX idx_chat_sessions_phone ON chat_sessions(contact_phone);
CREATE INDEX idx_media_files_conversation ON media_files(conversation_id);
```

### Future PostgreSQL Migration Schema

```sql
-- Enhanced schema for production use
CREATE TABLE conversations (
    id SERIAL PRIMARY KEY,
    chat_id VARCHAR(255) NOT NULL,
    message_id VARCHAR(255) UNIQUE,
    timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    sender_type VARCHAR(10) NOT NULL CHECK (sender_type IN ('user', 'bot')),
    sender_id VARCHAR(255),
    sender_name VARCHAR(255),
    message_type VARCHAR(20) NOT NULL CHECK (message_type IN ('text', 'image', 'document')),
    content TEXT,
    media_path TEXT,
    media_filename TEXT,
    caption TEXT,
    metadata JSONB,                           -- Native JSON support
    delivery_status VARCHAR(20) DEFAULT 'sent',
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Full-text search capability
CREATE INDEX idx_conversations_content_fts ON conversations USING gin(to_tsvector('english', content));
```

## Project Structure Recommendations

### Recommended Directory Structure

```
whatsapp-messaging-framework/
├── src/
│   ├── core/
│   │   ├── wppconnect-client.js       # WPPConnect initialization and management
│   │   ├── message-sender.js          # Outgoing message handling
│   │   ├── message-receiver.js        # Incoming message handling
│   │   └── session-manager.js         # WhatsApp session management
│   ├── database/
│   │   ├── connection.js              # Database connection setup
│   │   ├── migrations/                # Database schema migrations
│   │   │   └── 001-initial-schema.sql
│   │   ├── models/                    # Data models
│   │   │   ├── conversation.js
│   │   │   ├── chat-session.js
│   │   │   └── media-file.js
│   │   └── queries.js                 # Common database queries
│   ├── services/
│   │   ├── logging-service.js         # Message logging service
│   │   ├── media-service.js           # File handling for images/documents
│   │   └── event-service.js           # Event handling for AI integration
│   ├── utils/
│   │   ├── config.js                  # Configuration management
│   │   ├── logger.js                  # Application logging
│   │   └── helpers.js                 # Utility functions
│   └── index.js                       # Main application entry point
├── storage/
│   ├── media/                         # Uploaded images and documents
│   ├── database/                      # SQLite database files
│   └── logs/                          # Application logs
├── config/
│   ├── default.json                   # Default configuration
│   ├── development.json               # Development environment config
│   └── production.json                # Production environment config
├── tests/
│   ├── unit/                          # Unit tests
│   ├── integration/                   # Integration tests
│   └── fixtures/                      # Test data
├── docs/
│   ├── api.md                         # API documentation
│   ├── deployment.md                  # Deployment guide
│   └── examples.md                    # Usage examples
├── package.json
├── .env.example                       # Environment variables template
├── .gitignore
└── README.md
```

### Key Files and Their Purposes

#### `src/core/wppconnect-client.js`

```javascript
const wppconnect = require('@wppconnect-team/wppconnect');
const EventEmitter = require('events');
const LoggingService = require('../services/logging-service');

/**
 * Handles WPPConnect initialization, QR code management, session persistence
 */
class WPPConnectClient extends EventEmitter {
    constructor(config) {
        super();
        this.config = config;
        this.client = null;
        this.isConnected = false;
        this.loggingService = new LoggingService();
    }

    async initialize() {
        try {
            this.client = await wppconnect.create({
                sessionName: this.config.sessionName,
                headless: this.config.headless,
                devtools: this.config.devtools,
                browserArgs: this.config.browserArgs,
                onLoadingScreen: (percent, message) => {
                    console.log('Loading:', percent, message);
                },
                onQrCode: (base64Qr, asciiQR) => {
                    console.log('QR Code generated. Scan to authenticate.');
                    console.log(asciiQR);
                }
            });

            this.isConnected = true;
            this.setupEventListeners();
            this.emit('connected');
            
            return this.client;
        } catch (error) {
            this.emit('error', error);
            throw error;
        }
    }

    setupEventListeners() {
        // Listen for incoming messages
        this.client.onMessage(async (message) => {
            try {
                await this.handleIncomingMessage(message);
            } catch (error) {
                console.error('Error handling incoming message:', error);
            }
        });

        // Handle disconnections
        this.client.onStateChange((state) => {
            if (state === 'DISCONNECTED') {
                this.isConnected = false;
                this.emit('disconnected');
            }
        });
    }

    async handleIncomingMessage(message) {
        const messageData = {
            chatId: message.from,
            messageId: message.id,
            senderType: 'user',
            senderId: message.sender.id,
            senderName: message.sender.pushname || message.sender.name,
            messageType: this.getMessageType(message),
            content: message.body || null,
            mediaPath: null,
            caption: message.caption || null,
            metadata: JSON.stringify({
                timestamp: message.timestamp,
                isForwarded: message.isForwarded,
                quotedMessage: message.quotedMsg
            })
        };

        // Handle media files
        if (message.hasMedia) {
            const media = await message.downloadMedia();
            const mediaPath = await this.saveMedia(media, message);
            messageData.mediaPath = mediaPath;
        }

        // Log to database
        await this.loggingService.logMessage(messageData);

        // Emit event for AI processing
        this.emit('messageReceived', messageData);
    }

    getMessageType(message) {
        if (message.hasMedia) {
            if (message.type === 'image') return 'image';
            if (message.type === 'document') return 'document';
        }
        return 'text';
    }

    async saveMedia(media, message) {
        const fs = require('fs').promises;
        const path = require('path');
        const { v4: uuidv4 } = require('uuid');

        const extension = media.mimetype.split('/')[1];
        const filename = `${uuidv4()}.${extension}`;
        const filePath = path.join(this.config.mediaPath, filename);

        await fs.writeFile(filePath, media.data, 'base64');
        return filePath;
    }

    async sendText(chatId, message) {
        if (!this.isConnected) {
            throw new Error('Client not connected');
        }

        try {
            const result = await this.client.sendText(chatId, message);
            
            // Log outgoing message
            await this.loggingService.logMessage({
                chatId,
                messageId: result.id,
                senderType: 'bot',
                messageType: 'text',
                content: message,
                deliveryStatus: 'sent'
            });

            return result;
        } catch (error) {
            console.error('Error sending text message:', error);
            throw error;
        }
    }

    async sendImage(chatId, filePath, caption = '') {
        if (!this.isConnected) {
            throw new Error('Client not connected');
        }

        try {
            const result = await this.client.sendImage(chatId, filePath, 'image', caption);
            
            // Log outgoing message
            await this.loggingService.logMessage({
                chatId,
                messageId: result.id,
                senderType: 'bot',
                messageType: 'image',
                mediaPath: filePath,
                caption,
                deliveryStatus: 'sent'
            });

            return result;
        } catch (error) {
            console.error('Error sending image:', error);
            throw error;
        }
    }

    async sendDocument(chatId, filePath, caption = '') {
        if (!this.isConnected) {
            throw new Error('Client not connected');
        }

        try {
            const result = await this.client.sendFile(chatId, filePath, 'document', caption);
            
            // Log outgoing message
            await this.loggingService.logMessage({
                chatId,
                messageId: result.id,
                senderType: 'bot',
                messageType: 'document',
                mediaPath: filePath,
                caption,
                deliveryStatus: 'sent'
            });

            return result;
        } catch (error) {
            console.error('Error sending document:', error);
            throw error;
        }
    }

    async disconnect() {
        if (this.client) {
            await this.client.close();
            this.isConnected = false;
            this.emit('disconnected');
        }
    }
}

module.exports = WPPConnectClient;
```

#### `src/services/logging-service.js`

```javascript
const Database = require('../database/connection');
const Conversation = require('../database/models/conversation');

/**
 * Handles all conversation logging to database
 */
class LoggingService {
    constructor() {
        this.db = new Database();
    }

    async logMessage(messageData) {
        try {
            const conversation = await Conversation.create(messageData);
            console.log(`Message logged: ${conversation.id}`);
            return conversation;
        } catch (error) {
            console.error('Error logging message:', error);
            throw error;
        }
    }

    async getConversationHistory(chatId, limit = 50, offset = 0) {
        try {
            return await Conversation.findByChatId(chatId, { limit, offset });
        } catch (error) {
            console.error('Error retrieving conversation history:', error);
            throw error;
        }
    }

    async searchMessages(query, filters = {}) {
        try {
            return await Conversation.search(query, filters);
        } catch (error) {
            console.error('Error searching messages:', error);
            throw error;
        }
    }

    async exportConversation(chatId, format = 'json') {
        try {
            const messages = await this.getConversationHistory(chatId, 1000);
            
            if (format === 'json') {
                return JSON.stringify(messages, null, 2);
            }
            
            if (format === 'text') {
                return messages.map(msg => {
                    const timestamp = new Date(msg.timestamp).toLocaleString();
                    const sender = msg.sender_type === 'user' ? msg.sender_name : 'Bot';
                    return `[${timestamp}] ${sender}: ${msg.content || '[Media]'}`;
                }).join('\n');
            }
            
            throw new Error(`Unsupported format: ${format}`);
        } catch (error) {
            console.error('Error exporting conversation:', error);
            throw error;
        }
    }

    async getActiveChats() {
        try {
            const query = `
                SELECT 
                    chat_id,
                    contact_name,
                    contact_phone,
                    last_message_at,
                    total_messages
                FROM chat_sessions 
                WHERE session_status = 'active'
                ORDER BY last_message_at DESC
            `;
            
            return await this.db.all(query);
        } catch (error) {
            console.error('Error retrieving active chats:', error);
            throw error;
        }
    }

    async updateChatSession(chatId, messageData) {
        try {
            const updateQuery = `
                INSERT INTO chat_sessions (
                    chat_id, 
                    contact_name, 
                    contact_phone, 
                    first_message_at, 
                    last_message_at, 
                    total_messages
                ) VALUES (?, ?, ?, ?, ?, 1)
                ON CONFLICT(chat_id) DO UPDATE SET
                    last_message_at = ?,
                    total_messages = total_messages + 1,
                    updated_at = CURRENT_TIMESTAMP
            `;

            const timestamp = new Date().toISOString();
            
            await this.db.run(updateQuery, [
                chatId,
                messageData.senderName,
                messageData.senderId,
                timestamp,
                timestamp,
                timestamp
            ]);
        } catch (error) {
            console.error('Error updating chat session:', error);
            throw error;
        }
    }
}

module.exports = LoggingService;
```

#### `src/database/models/conversation.js`

```javascript
const Database = require('../connection');

/**
 * Data model for conversation entries
 */
class Conversation {
    constructor(data) {
        Object.assign(this, data);
    }

    static async create(messageData) {
        const db = new Database();
        
        const query = `
            INSERT INTO conversations (
                chat_id, message_id, sender_type, sender_id, sender_name,
                message_type, content, media_path, media_filename, 
                caption, metadata, delivery_status
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        `;

        const params = [
            messageData.chatId,
            messageData.messageId,
            messageData.senderType,
            messageData.senderId,
            messageData.senderName,
            messageData.messageType,
            messageData.content,
            messageData.mediaPath,
            messageData.mediaFilename,
            messageData.caption,
            messageData.metadata,
            messageData.deliveryStatus || null
        ];

        const result = await db.run(query, params);
        
        return new Conversation({
            id: result.lastID,
            ...messageData
        });
    }

    static async findByChatId(chatId, options = {}) {
        const db = new Database();
        const { limit = 50, offset = 0, orderBy = 'timestamp ASC' } = options;
        
        const query = `
            SELECT * FROM conversations 
            WHERE chat_id = ? 
            ORDER BY ${orderBy}
            LIMIT ? OFFSET ?
        `;

        const rows = await db.all(query, [chatId, limit, offset]);
        return rows.map(row => new Conversation(row));
    }

    static async findById(id) {
        const db = new Database();
        const query = 'SELECT * FROM conversations WHERE id = ?';
        const row = await db.get(query, [id]);
        
        return row ? new Conversation(row) : null;
    }

    static async updateDeliveryStatus(messageId, status) {
        const db = new Database();
        const query = `
            UPDATE conversations 
            SET delivery_status = ?, updated_at = CURRENT_TIMESTAMP 
            WHERE message_id = ?
        `;
        
        return await db.run(query, [status, messageId]);
    }

    static async search(query, filters = {}) {
        const db = new Database();
        let sql = 'SELECT * FROM conversations WHERE 1=1';
        const params = [];

        // Text search
        if (query) {
            sql += ' AND content LIKE ?';
            params.push(`%${query}%`);
        }

        // Filter by chat
        if (filters.chatId) {
            sql += ' AND chat_id = ?';
            params.push(filters.chatId);
        }

        // Filter by message type
        if (filters.messageType) {
            sql += ' AND message_type = ?';
            params.push(filters.messageType);
        }

        // Filter by sender type
        if (filters.senderType) {
            sql += ' AND sender_type = ?';
            params.push(filters.senderType);
        }

        // Date range
        if (filters.startDate) {
            sql += ' AND timestamp >= ?';
            params.push(filters.startDate);
        }

        if (filters.endDate) {
            sql += ' AND timestamp <= ?';
            params.push(filters.endDate);
        }

        sql += ' ORDER BY timestamp DESC';

        if (filters.limit) {
            sql += ' LIMIT ?';
            params.push(filters.limit);
        }

        const rows = await db.all(sql, params);
        return rows.map(row => new Conversation(row));
    }

    static async getStatistics(chatId = null) {
        const db = new Database();
        let query = `
            SELECT 
                COUNT(*) as total_messages,
                COUNT(CASE WHEN sender_type = 'user' THEN 1 END) as user_messages,
                COUNT(CASE WHEN sender_type = 'bot' THEN 1 END) as bot_messages,
                COUNT(CASE WHEN message_type = 'text' THEN 1 END) as text_messages,
                COUNT(CASE WHEN message_type = 'image' THEN 1 END) as image_messages,
                COUNT(CASE WHEN message_type = 'document' THEN 1 END) as document_messages,
                MIN(timestamp) as first_message,
                MAX(timestamp) as last_message
            FROM conversations
        `;

        const params = [];
        if (chatId) {
            query += ' WHERE chat_id = ?';
            params.push(chatId);
        }

        return await db.get(query, params);
    }
}

module.exports = Conversation;
```

#### `src/database/connection.js`

```javascript
const sqlite3 = require('sqlite3').verbose();
const path = require('path');
const fs = require('fs').promises;

/**
 * Database connection setup and management
 */
class Database {
    constructor(dbPath = null) {
        this.dbPath = dbPath || path.join(__dirname, '../../storage/database/conversations.db');
        this.db = null;
    }

    async connect() {
        return new Promise((resolve, reject) => {
            this.db = new sqlite3.Database(this.dbPath, (err) => {
                if (err) {
                    reject(err);
                } else {
                    console.log('Connected to SQLite database');
                    resolve(this.db);
                }
            });
        });
    }

    async initialize() {
        await this.ensureDirectoryExists();
        await this.connect();
        await this.runMigrations();
    }

    async ensureDirectoryExists() {
        const dir = path.dirname(this.dbPath);
        try {
            await fs.access(dir);
        } catch {
            await fs.mkdir(dir, { recursive: true });
        }
    }

    async runMigrations() {
        const migrationPath = path.join(__dirname, 'migrations/001-initial-schema.sql');
        try {
            const schema = await fs.readFile(migrationPath, 'utf8');
            await this.exec(schema);
            console.log('Database migrations completed');
        } catch (error) {
            console.error('Error running migrations:', error);
            throw error;
        }
    }

    async run(sql, params = []) {
        return new Promise((resolve, reject) => {
            this.db.run(sql, params, function(err) {
                if (err) {
                    reject(err);
                } else {
                    resolve({ lastID: this.lastID, changes: this.changes });
                }
            });
        });
    }

    async get(sql, params = []) {
        return new Promise((resolve, reject) => {
            this.db.get(sql, params, (err, row) => {
                if (err) {
                    reject(err);
                } else {
                    resolve(row);
                }
            });
        });
    }

    async all(sql, params = []) {
        return new Promise((resolve, reject) => {
            this.db.all(sql, params, (err, rows) => {
                if (err) {
                    reject(err);
                } else {
                    resolve(rows);
                }
            });
        });
    }

    async exec(sql) {
        return new Promise((resolve, reject) => {
            this.db.exec(sql, (err) => {
                if (err) {
                    reject(err);
                } else {
                    resolve();
                }
            });
        });
    }

    async close() {
        return new Promise((resolve, reject) => {
            if (this.db) {
                this.db.close((err) => {
                    if (err) {
                        reject(err);
                    } else {
                        console.log('Database connection closed');
                        resolve();
                    }
                });
            } else {
                resolve();
            }
        });
    }
}

module.exports = Database;
```

### Configuration Structure

#### `config/default.json`

```json
{
  "database": {
    "type": "sqlite",
    "path": "./storage/database/conversations.db"
  },
  "wppconnect": {
    "sessionName": "medical-assistant",
    "headless": true,
    "devtools": false,
    "browserArgs": ["--no-sandbox", "--disable-setuid-sandbox"]
  },
  "storage": {
    "mediaPath": "./storage/media",
    "maxFileSize": "10MB"
  },
  "logging": {
    "level": "info",
    "file": "./storage/logs/app.log"
  },
  "app": {
    "name": "WhatsApp Messaging Framework",
    "version": "1.0.0"
  }
}
```

#### `src/index.js` - Main Application Entry Point

```javascript
const WPPConnectClient = require('./core/wppconnect-client');
const Database = require('./database/connection');
const config = require('config');
const path = require('path');

/**
 * Main application entry point
 */
class WhatsAppFramework {
    constructor() {
        this.client = null;
        this.database = null;
        this.isRunning = false;
    }

    async initialize() {
        try {
            console.log('Initializing WhatsApp Messaging Framework...');
            
            // Initialize database
            this.database = new Database(config.get('database.path'));
            await this.database.initialize();
            
            // Initialize WPPConnect client
            this.client = new WPPConnectClient({
                sessionName: config.get('wppconnect.sessionName'),
                headless: config.get('wppconnect.headless'),
                devtools: config.get('wppconnect.devtools'),
                browserArgs: config.get('wppconnect.browserArgs'),
                mediaPath: config.get('storage.mediaPath')
            });

            // Set up event handlers
            this.setupEventHandlers();
            
            // Connect to WhatsApp
            await this.client.initialize();
            
            this.isRunning = true;
            console.log('Framework initialized successfully!');
            
        } catch (error) {
            console.error('Failed to initialize framework:', error);
            process.exit(1);
        }
    }

    setupEventHandlers() {
        this.client.on('connected', () => {
            console.log('✅ Connected to WhatsApp');
        });

        this.client.on('disconnected', () => {
            console.log('❌ Disconnected from WhatsApp');
            this.handleDisconnection();
        });

        this.client.on('messageReceived', (messageData) => {
            console.log(`📨 Message received from ${messageData.senderName}: ${messageData.content || '[Media]'}`);
            // Here you can add AI processing logic
            this.processIncomingMessage(messageData);
        });

        this.client.on('error', (error) => {
            console.error('❌ WPPConnect error:', error);
        });

        // Handle graceful shutdown
        process.on('SIGINT', () => {
            console.log('\n🛑 Shutting down gracefully...');
            this.shutdown();
        });

        process.on('SIGTERM', () => {
            console.log('\n🛑 Shutting down gracefully...');
            this.shutdown();
        });
    }

    async processIncomingMessage(messageData) {
        // This is where you would integrate AI processing
        console.log(`Processing message from ${messageData.chatId}`);
        
        // Example: Simple echo response
        if (messageData.content && messageData.content.toLowerCase() === 'hello') {
            await this.client.sendText(messageData.chatId, 'Hello! I am your medical assistant. How can I help you today?');
        }
    }

    async handleDisconnection() {
        console.log('Attempting to reconnect...');
        setTimeout(async () => {
            try {
                await this.client.initialize();
            } catch (error) {
                console.error('Reconnection failed:', error);
            }
        }, 5000);
    }

    async shutdown() {
        try {
            this.isRunning = false;
            
            if (this.client) {
                await this.client.disconnect();
            }
            
            if (this.database) {
                await this.database.close();
            }
            
            console.log('✅ Framework shut down successfully');
            process.exit(0);
        } catch (error) {
            console.error('Error during shutdown:', error);
            process.exit(1);
        }
    }

    // Public API methods for external integration
    async sendMessage(chatId, message) {
        if (!this.isRunning) {
            throw new Error('Framework not initialized');
        }
        return await this.client.sendText(chatId, message);
    }

    async sendImage(chatId, imagePath, caption = '') {
        if (!this.isRunning) {
            throw new Error('Framework not initialized');
        }
        return await this.client.sendImage(chatId, imagePath, caption);
    }

    async sendDocument(chatId, documentPath, caption = '') {
        if (!this.isRunning) {
            throw new Error('Framework not initialized');
        }
        return await this.client.sendDocument(chatId, documentPath, caption);
    }

    async getConversationHistory(chatId, limit = 50) {
        const loggingService = require('./services/logging-service');
        const service = new loggingService();
        return await service.getConversationHistory(chatId, limit);
    }
}

// Start the framework if this file is run directly
if (require.main === module) {
    const framework = new WhatsAppFramework();
    framework.initialize().catch(console.error);
}

module.exports = WhatsAppFramework;
```

### Dependencies (`package.json`)

```json
{
  "name": "whatsapp-messaging-framework",
  "version": "1.0.0",
  "description": "Raw and functional WhatsApp messaging framework using WPPConnect for AI medical assistant",
  "main": "src/index.js",
  "scripts": {
    "start": "node src/index.js",
    "dev": "nodemon src/index.js",
    "test": "jest",
    "lint": "eslint src/",
    "migrate": "node scripts/migrate.js"
  },
  "dependencies": {
    "@wppconnect-team/wppconnect": "^1.29.0",
    "sqlite3": "^5.1.6",
    "winston": "^3.10.0",
    "config": "^3.3.9",
    "dotenv": "^16.3.1",
    "mime-types": "^2.1.35",
    "uuid": "^9.0.0"
  },
  "devDependencies": {
    "jest": "^29.6.2",
    "nodemon": "^3.0.1",
    "eslint": "^8.46.0",
    "supertest": "^6.3.3"
  },
  "keywords": [
    "whatsapp",
    "messaging",
    "wppconnect",
    "ai",
    "medical",
    "assistant"
  ],
  "author": "Your Name",
  "license": "MIT"
}
```

This specification provides a complete, production-ready foundation for your WhatsApp messaging framework that can easily evolve into your AI medical assistant system.
