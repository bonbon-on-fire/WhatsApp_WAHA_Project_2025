# WhatsApp WPPConnect CLI Integration - Technical Design Document

## 1. Design Overview

### 1.1 System Architecture

The WhatsApp WPPConnect CLI integration system consists of two main components:

1. **WPPConnect Server**: Third-party service that handles WhatsApp Web connection
2. **CLI Application**: Node.js command-line tool that interfaces with WPPConnect Server

### 1.2 High-Level Architecture Diagram

```text
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   CLI Commands  │───►│  CLI Application │◄──►│  WPPConnect     │
│   (User Input)  │    │   (Node.js)     │    │    Server       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │                       │
                                │                       │
                                ▼                       ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   Terminal      │    │   WhatsApp      │
                       │   Output        │    │     Web         │
                       │   (Messages)    │    │                 │
                       └─────────────────┘    └─────────────────┘
```

### 1.3 Design Philosophy

- **Simplicity First**: Minimal dependencies and simple command-line interface
- **Real-time Communication**: Immediate display of incoming messages in terminal
- **Surgical Changes**: Build a lightweight integration layer without modifying WPPConnect Server
- **Single Purpose**: Focus solely on message sending and receiving via CLI

## 2. Component Design

### 2.1 CLI Application Structure

```typescript
// Project structure
src/
├── commands/
│   ├── start.ts
│   ├── stop.ts
│   ├── send.ts
│   ├── listen.ts
│   ├── status.ts
│   └── index.ts
├── services/
│   ├── wppConnectService.ts
│   ├── messageService.ts
│   └── webhookService.ts
├── utils/
│   ├── config.ts
│   ├── logger.ts
│   ├── formatter.ts
│   └── qrCode.ts
├── types/
│   ├── message.ts
│   ├── session.ts
│   └── config.ts
└── cli.ts (main entry point)
```

### 2.2 CLI Command Structure

#### 2.2.1 Main CLI Entry Point

```typescript
// cli.ts
import { Command } from 'commander';
import { startCommand } from './commands/start';
import { stopCommand } from './commands/stop';
import { sendCommand } from './commands/send';
import { listenCommand } from './commands/listen';
import { statusCommand } from './commands/status';

const program = new Command();

program
  .name('whatsapp')
  .description('WhatsApp CLI Integration Tool')
  .version('1.0.0');

program
  .command('start')
  .description('Start WhatsApp session')
  .option('-s, --session <name>', 'Session name', 'default')
  .action(startCommand);

program
  .command('stop')
  .description('Stop WhatsApp session')
  .option('-s, --session <name>', 'Session name', 'default')
  .action(stopCommand);

program
  .command('send')
  .description('Send message to contact')
  .argument('<phone>', 'Phone number')
  .argument('<message>', 'Message text')
  .option('-s, --session <name>', 'Session name', 'default')
  .action(sendCommand);

program
  .command('listen')
  .description('Listen for incoming messages')
  .option('-s, --session <name>', 'Session name', 'default')
  .option('-f, --format <type>', 'Output format: simple|json|detailed', 'simple')
  .action(listenCommand);

program
  .command('status')
  .description('Show connection status')
  .option('-s, --session <name>', 'Session name', 'default')
  .action(statusCommand);

program.parse();
```

#### 2.2.2 Command Implementations

```typescript
// commands/start.ts
export async function startCommand(options: { session: string }) {
  const wppService = new WPPConnectService();
  
  try {
    console.log(`Starting WhatsApp session: ${options.session}`);
    
    // Generate authentication token
    const token = await wppService.generateToken(options.session);
    console.log('✓ Authentication token generated');
    
    // Start session
    const sessionData = await wppService.startSession(options.session);
    
    if (sessionData.qrCode) {
      console.log('\nScan QR code with your phone:');
      displayQRCode(sessionData.qrCode);
      
      // Wait for authentication
      await waitForAuthentication(options.session);
      console.log('✓ Successfully authenticated!');
    }
    
    console.log(`✓ Session ${options.session} started successfully`);
  } catch (error) {
    console.error('✗ Failed to start session:', error.message);
    process.exit(1);
  }
}

// commands/send.ts
export async function sendCommand(
  phone: string, 
  message: string, 
  options: { session: string }
) {
  const wppService = new WPPConnectService();
  
  try {
    // Validate phone number
    if (!isValidPhoneNumber(phone)) {
      throw new Error('Invalid phone number format');
    }
    
    console.log(`Sending message to ${phone}...`);
    
    const result = await wppService.sendMessage(options.session, {
      phone,
      message
    });
    
    if (result.success) {
      console.log('✓ Message sent successfully');
      if (result.messageId) {
        console.log(`Message ID: ${result.messageId}`);
      }
    } else {
      console.error('✗ Failed to send message:', result.message);
    }
  } catch (error) {
    console.error('✗ Error sending message:', error.message);
    process.exit(1);
  }
}

// commands/listen.ts
export async function listenCommand(options: { 
  session: string; 
  format: 'simple' | 'json' | 'detailed' 
}) {
  const messageService = new MessageService();
  
  console.log('Listening for incoming messages... (Press Ctrl+C to stop)');
  
  // Start webhook server
  const webhookServer = new WebhookService();
  await webhookServer.start();
  
  // Listen for messages
  messageService.on('message', (message) => {
    displayMessage(message, options.format);
  });
  
  messageService.on('status', (status) => {
    if (options.format === 'detailed') {
      console.log(`[STATUS] ${status.type}: ${status.message}`);
    }
  });
  
  // Handle graceful shutdown
  process.on('SIGINT', async () => {
    console.log('\nStopping message listener...');
    await webhookServer.stop();
    process.exit(0);
  });
}
```

### 2.3 WPPConnect Service Layer

```typescript
// services/wppConnectService.ts
import axios, { AxiosInstance } from 'axios';
import { config } from '../utils/config';

export class WPPConnectService {
  private api: AxiosInstance;
  private baseUrl: string;
  private secretKey: string;
  private tokens: Map<string, string> = new Map();

  constructor() {
    this.baseUrl = config.wppconnect.baseUrl;
    this.secretKey = config.wppconnect.secretKey;
    
    this.api = axios.create({
      baseURL: this.baseUrl,
      timeout: 30000,
    });
  }

  async generateToken(sessionName: string): Promise<string> {
    try {
      const response = await this.api.post(
        `/api/${sessionName}/${this.secretKey}/generate-token`
      );
      
      const token = response.data.token;
      this.tokens.set(sessionName, token);
      
      return token;
    } catch (error) {
      throw new Error(`Failed to generate token: ${error.message}`);
    }
  }

  async startSession(sessionName: string): Promise<SessionResponse> {
    const token = this.tokens.get(sessionName);
    if (!token) {
      throw new Error('No authentication token found. Run generate-token first.');
    }

    try {
      const response = await this.api.post(
        `/api/${sessionName}/start-session`,
        {},
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );

      return response.data;
    } catch (error) {
      throw new Error(`Failed to start session: ${error.message}`);
    }
  }

  async sendMessage(
    sessionName: string, 
    messageData: SendMessageRequest
  ): Promise<MessageResponse> {
    const token = this.tokens.get(sessionName);
    if (!token) {
      throw new Error('No authentication token found. Start session first.');
    }

    try {
      const response = await this.api.post(
        `/api/${sessionName}/send-message`,
        messageData,
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );

      return response.data;
    } catch (error) {
      throw new Error(`Failed to send message: ${error.message}`);
    }
  }

  async checkConnectionStatus(sessionName: string): Promise<ConnectionStatus> {
    const token = this.tokens.get(sessionName);
    if (!token) {
      throw new Error('No authentication token found.');
    }

    try {
      const response = await this.api.get(
        `/api/${sessionName}/check-connection-session`,
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );

      return response.data;
    } catch (error) {
      throw new Error(`Failed to check connection: ${error.message}`);
    }
  }
}
```

### 2.4 Webhook Service for Receiving Messages

```typescript
// services/webhookService.ts
import express from 'express';
import { EventEmitter } from 'events';
import { MessageService } from './messageService';
import { config } from '../utils/config';

export class WebhookService extends EventEmitter {
  private app: express.Application;
  private server: any;
  private messageService: MessageService;

  constructor() {
    super();
    this.app = express();
    this.messageService = new MessageService();
    this.setupMiddleware();
    this.setupRoutes();
  }

  private setupMiddleware() {
    this.app.use(express.json({ limit: '50mb' }));
    this.app.use(express.urlencoded({ extended: true }));
  }

  private setupRoutes() {
    this.app.post('/webhook', (req, res) => {
      try {
        const messageData = req.body;
        this.messageService.processIncomingMessage(messageData);
        res.status(200).json({ status: 'received' });
      } catch (error) {
        console.error('Webhook error:', error);
        res.status(500).json({ error: 'Processing failed' });
      }
    });

    this.app.get('/health', (req, res) => {
      res.json({ status: 'healthy', timestamp: new Date().toISOString() });
    });
  }

  async start(): Promise<void> {
    return new Promise((resolve) => {
      const port = config.webhook.port || 3000;
      this.server = this.app.listen(port, () => {
        console.log(`Webhook server listening on port ${port}`);
        resolve();
      });
    });
  }

  async stop(): Promise<void> {
    return new Promise((resolve) => {
      if (this.server) {
        this.server.close(() => {
          console.log('Webhook server stopped');
          resolve();
        });
      } else {
        resolve();
      }
    });
  }
}
```

### 2.5 Message Processing and Display

```typescript
// services/messageService.ts
import { EventEmitter } from 'events';
import { formatMessage } from '../utils/formatter';

export class MessageService extends EventEmitter {
  
  processIncomingMessage(messageData: WebhookMessageData): void {
    try {
      const message = this.parseMessage(messageData);
      
      // Emit message event for listeners
      this.emit('message', message);
      
      // Log message for debugging
      console.log('Processed message:', message.id);
    } catch (error) {
      console.error('Error processing message:', error);
      this.emit('error', error);
    }
  }

  private parseMessage(data: WebhookMessageData): Message {
    return {
      id: data.data.id,
      content: data.data.body,
      sender: data.data.senderName || data.data.from,
      senderPhone: data.data.from,
      timestamp: new Date(data.data.timestamp * 1000),
      messageType: data.data.type as MessageType,
      isFromMe: data.data.fromMe,
      chatName: data.data.chatName,
      isGroup: data.data.from.includes('@g.us'),
    };
  }
}

// utils/formatter.ts
export function displayMessage(message: Message, format: string): void {
  const timestamp = message.timestamp.toLocaleString();
  
  switch (format) {
    case 'json':
      console.log(JSON.stringify(message, null, 2));
      break;
      
    case 'detailed':
      console.log('─'.repeat(50));
      console.log(`From: ${message.sender} (${message.senderPhone})`);
      console.log(`Time: ${timestamp}`);
      console.log(`Type: ${message.messageType}`);
      if (message.isGroup) {
        console.log(`Group: ${message.chatName}`);
      }
      console.log(`Message: ${message.content}`);
      console.log('─'.repeat(50));
      break;
      
    case 'simple':
    default:
      const prefix = message.isGroup ? `${message.chatName}: ${message.sender}` : message.sender;
      console.log(`[${timestamp}] ${prefix}: ${message.content}`);
      break;
  }
}

export function displayQRCode(qrCode: string): void {
  // Convert QR code to terminal display
  const QRCode = require('qrcode');
  
  QRCode.toString(qrCode, { type: 'terminal' }, (err: any, qrString: string) => {
    if (err) {
      console.error('Failed to display QR code:', err);
      console.log('QR Code Data:', qrCode);
    } else {
      console.log(qrString);
    }
  });
}
```

## 3. Data Models

### 3.1 Core Data Structures

```typescript
// types/message.ts
export interface Message {
  id: string;
  content: string;
  sender: string;
  senderPhone: string;
  timestamp: Date;
  messageType: MessageType;
  isFromMe: boolean;
  chatName?: string;
  isGroup: boolean;
  mediaUrl?: string;
  mediaData?: MediaData;
}

export type MessageType = 'text' | 'image' | 'document' | 'audio' | 'video' | 'sticker';

export interface MediaData {
  filename: string;
  mimetype: string;
  size: number;
  data?: string; // base64 encoded
}

// types/session.ts
export interface SessionResponse {
  success: boolean;
  message: string;
  qrCode?: string;
  sessionName: string;
  connected: boolean;
}

export interface ConnectionStatus {
  connected: boolean;
  phoneNumber?: string;
  batteryLevel?: number;
  platform?: string;
  lastSeen?: Date;
}

// types/config.ts
export interface Config {
  wppconnect: {
    baseUrl: string;
    secretKey: string;
  };
  webhook: {
    port: number;
    path: string;
  };
  logging: {
    level: string;
    file?: string;
  };
}
```

### 3.2 API Request/Response Models

```typescript
export interface SendMessageRequest {
  phone: string;
  message: string;
  isGroup?: boolean;
}

export interface MessageResponse {
  success: boolean;
  message: string;
  messageId?: string;
  error?: string;
}

export interface WebhookMessageData {
  instanceName: string;
  data: {
    id: string;
    body: string;
    type: string;
    timestamp: number;
    from: string;
    fromMe: boolean;
    to: string;
    chatName: string;
    senderName: string;
    quotedMsg?: any;
    mediaData?: {
      filename: string;
      mimetype: string;
      data: string;
    };
  };
}
```

## 4. Configuration Management

### 4.1 Configuration File Structure

```typescript
// utils/config.ts
import { Config } from '../types/config';
import path from 'path';
import fs from 'fs';

const configPath = path.join(process.cwd(), 'whatsapp.config.json');

const defaultConfig: Config = {
  wppconnect: {
    baseUrl: 'http://localhost:21465',
    secretKey: process.env.WPPCONNECT_SECRET || 'your-secret-key'
  },
  webhook: {
    port: 3000,
    path: '/webhook'
  },
  logging: {
    level: 'info',
    file: 'whatsapp-cli.log'
  }
};

export function loadConfig(): Config {
  try {
    if (fs.existsSync(configPath)) {
      const configFile = fs.readFileSync(configPath, 'utf8');
      const userConfig = JSON.parse(configFile);
      return { ...defaultConfig, ...userConfig };
    }
  } catch (error) {
    console.warn('Warning: Could not load config file, using defaults');
  }
  
  return defaultConfig;
}

export function saveConfig(config: Config): void {
  try {
    fs.writeFileSync(configPath, JSON.stringify(config, null, 2));
  } catch (error) {
    console.error('Failed to save config:', error.message);
  }
}

export const config = loadConfig();
```

## 5. Error Handling Strategy

### 5.1 Error Categories and Handling

```typescript
// utils/errors.ts
export class WhatsAppCLIError extends Error {
  constructor(
    message: string,
    public code: string,
    public statusCode?: number
  ) {
    super(message);
    this.name = 'WhatsAppCLIError';
  }
}

export class ConnectionError extends WhatsAppCLIError {
  constructor(message: string) {
    super(message, 'CONNECTION_ERROR', 503);
  }
}

export class AuthenticationError extends WhatsAppCLIError {
  constructor(message: string) {
    super(message, 'AUTH_ERROR', 401);
  }
}

export class ValidationError extends WhatsAppCLIError {
  constructor(message: string) {
    super(message, 'VALIDATION_ERROR', 400);
  }
}

export function handleError(error: Error): void {
  if (error instanceof WhatsAppCLIError) {
    console.error(`✗ ${error.message}`);
    if (process.env.DEBUG) {
      console.error(`Code: ${error.code}`);
    }
  } else {
    console.error('✗ Unexpected error:', error.message);
    if (process.env.DEBUG) {
      console.error(error.stack);
    }
  }
}
```

## 6. Logging and Monitoring

### 6.1 Logging Implementation

```typescript
// utils/logger.ts
import winston from 'winston';
import { config } from './config';

export const logger = winston.createLogger({
  level: config.logging.level,
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console({
      format: winston.format.combine(
        winston.format.colorize(),
        winston.format.simple()
      )
    })
  ]
});

// Add file logging if configured
if (config.logging.file) {
  logger.add(new winston.transports.File({
    filename: config.logging.file,
    level: 'info'
  }));
  
  logger.add(new winston.transports.File({
    filename: 'error.log',
    level: 'error'
  }));
}
```

## 7. Installation and Distribution

### 7.1 NPM Package Configuration

```json
// package.json
{
  "name": "whatsapp-cli",
  "version": "1.0.0",
  "description": "Command-line WhatsApp integration using WPPConnect",
  "main": "dist/cli.js",
  "bin": {
    "whatsapp": "./dist/cli.js"
  },
  "scripts": {
    "build": "tsc",
    "dev": "ts-node src/cli.ts",
    "start": "node dist/cli.js",
    "test": "jest",
    "prepublish": "npm run build"
  },
  "dependencies": {
    "commander": "^9.0.0",
    "axios": "^1.0.0",
    "express": "^4.18.0",
    "winston": "^3.8.0",
    "qrcode": "^1.5.0",
    "qrcode-terminal": "^0.12.0"
  },
  "devDependencies": {
    "@types/node": "^18.0.0",
    "typescript": "^4.8.0",
    "ts-node": "^10.9.0",
    "jest": "^29.0.0",
    "@types/jest": "^29.0.0"
  },
  "engines": {
    "node": ">=16.0.0"
  },
  "keywords": [
    "whatsapp",
    "cli",
    "wppconnect",
    "messaging"
  ]
}
```

### 7.2 Build and Distribution

```bash
# Build process
npm run build

# Global installation
npm install -g whatsapp-cli

# Local installation
npm install whatsapp-cli

# Usage after installation
whatsapp start
whatsapp send +1234567890 "Hello World"
whatsapp listen
```

## 8. Testing Strategy

### 8.1 Unit Testing Examples

```typescript
// tests/services/wppConnectService.test.ts
import { WPPConnectService } from '../../src/services/wppConnectService';

describe('WPPConnectService', () => {
  let service: WPPConnectService;

  beforeEach(() => {
    service = new WPPConnectService();
  });

  it('should generate authentication token', async () => {
    const mockResponse = { data: { token: 'test-token' } };
    jest.spyOn(service['api'], 'post').mockResolvedValue(mockResponse);

    const token = await service.generateToken('test-session');
    
    expect(token).toBe('test-token');
  });

  it('should send message successfully', async () => {
    const mockResponse = { 
      data: { 
        success: true, 
        messageId: 'msg-123' 
      } 
    };
    
    jest.spyOn(service['api'], 'post').mockResolvedValue(mockResponse);

    const result = await service.sendMessage('test-session', {
      phone: '+1234567890',
      message: 'Test message'
    });

    expect(result.success).toBe(true);
    expect(result.messageId).toBe('msg-123');
  });
});
```

## 9. Deployment and Production

### 9.1 Production Considerations

```bash
# PM2 process management
pm2 start ecosystem.config.js

# Environment variables
export WPPCONNECT_SECRET="your-production-secret"
export NODE_ENV="production"

# Logging
mkdir -p /var/log/whatsapp-cli
chown app:app /var/log/whatsapp-cli

# Systemd service (optional)
sudo systemctl enable whatsapp-cli
sudo systemctl start whatsapp-cli
```

This simplified design focuses entirely on CLI functionality while maintaining the core WhatsApp integration capabilities you need. The architecture is much simpler, easier to implement, and perfectly suited for terminal-based message management.
