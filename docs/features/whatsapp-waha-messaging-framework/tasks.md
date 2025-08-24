# Task List: WhatsApp WAHA Messaging Framework

## Overview
Implementation tasks for the WhatsApp WAHA messaging framework, organized by priority and dependencies. Each task includes subtasks, requirements mapping, and test criteria.

---

## Task 1: Project Setup and Core Infrastructure [✅ 100% Complete]
- [x] **Create solution structure with .NET Core 9.0 projects**
  - [x] Create solution file `WhatsAppWahaFramework.sln`
  - [x] Create `WhatsAppWaha.Core` class library project
  - [x] Create `WhatsAppWaha.HelloWorld` console application project
  - [x] Create `WhatsAppWaha.MessageReceiver` console application project
  - [x] Create test projects for each component
  - [x] Configure project references and dependencies
  - **Requirements:**
    - [x] Requirement 6: .NET Core 9.0 Modern Architecture
  - **Tests:**
    - [x] Test 1: Verify all projects build successfully
    - [x] Test 2: Verify project references are correctly configured
    - [x] Test 3: Verify .NET Core 9.0 target framework

- [x] **Set up dependency injection and configuration infrastructure**
  - [x] Create `ServiceCollectionExtensions.cs` for DI registration
  - [x] Implement configuration models (`WahaSettings`, `NtfySettings`, `AppSettings`)
  - [x] Set up configuration binding with validation
  - [x] Configure structured logging with Serilog
  - [x] Implement global exception handling patterns
  - **Requirements:**
    - [x] Requirement 6: .NET Core 9.0 Modern Architecture
    - [x] Requirement 8: Configuration Management
  - **Tests:**
    - [x] Test 1: Verify DI container resolves all services
    - [x] Test 2: Verify configuration binding works correctly
    - [x] Test 3: Verify logging writes to configured sinks
    - [x] **Added 70 comprehensive unit tests covering all infrastructure**

---

## Task 2: WAHA Integration Service Implementation
- [ ] **Implement core WAHA service with HTTP client**
  - [ ] Create `IWahaService` interface with required methods
  - [ ] Implement `WahaService` class with HTTP client
  - [ ] Add phone number validation logic
  - [ ] Implement message sending to `/api/sendText` endpoint
  - [ ] Add session validation functionality
  - **Requirements:**
    - [ ] Requirement 2: WAHA Integration Service
  - **Tests:**
    - [ ] Test 1: Verify phone number validation accepts valid formats
    - [ ] Test 2: Verify phone number validation rejects invalid formats
    - [ ] Test 3: Verify HTTP client sends correct JSON payload

- [ ] **Add retry policies and error handling**
  - [ ] Configure HTTP client with Polly retry policies
  - [ ] Implement exponential backoff for transient failures
  - [ ] Add circuit breaker pattern for WAHA availability
  - [ ] Create custom exceptions for different error scenarios
  - [ ] Add comprehensive logging for all operations
  - **Requirements:**
    - [ ] Requirement 7: Error Handling and Resilience
  - **Tests:**
    - [ ] Test 1: Verify retry logic activates on transient failures
    - [ ] Test 2: Verify circuit breaker opens after consecutive failures
    - [ ] Test 3: Verify appropriate exceptions are thrown for different errors

- [ ] **Create WAHA message models and response handling**
  - [ ] Implement `WahaMessage` model for sending messages
  - [ ] Implement `WahaMessageResult` model for API responses
  - [ ] Add JSON serialization attributes and converters
  - [ ] Implement response validation and error parsing
  - **Requirements:**
    - [ ] Requirement 2: WAHA Integration Service
  - **Tests:**
    - [ ] Test 1: Verify models serialize to correct JSON format
    - [ ] Test 2: Verify response models deserialize WAHA API responses
    - [ ] Test 3: Verify error responses are properly parsed

---

## Task 3: ntfy Service Implementation (Message Relay + Notifications)
- [ ] **Implement ntfy polling service for message retrieval**
  - [ ] Create `INtfyService` interface with polling and notification methods
  - [ ] Implement HTTP client for polling ntfy topics via GET requests
  - [ ] Add polling method to retrieve messages from ntfy JSON API
  - [ ] Implement message deduplication tracking with processed IDs
  - [ ] Add graceful handling of ntfy service unavailability
  - **Requirements:**
    - [ ] Requirement 3: Phase 2 - Message Receiver (revised for ntfy polling)
    - [ ] Requirement 5: ntfy Notification Integration
  - **Tests:**
    - [ ] Test 1: Verify polling retrieves messages from correct ntfy topic
    - [ ] Test 2: Verify message deduplication prevents duplicate processing
    - [ ] Test 3: Verify graceful handling when ntfy service is down

- [ ] **Implement ntfy notification service**
  - [ ] Add notification methods for different event types (success, error, info)
  - [ ] Implement fire-and-forget pattern to avoid blocking main processing
  - [ ] Add support for multiple ntfy topics (messages vs notifications)
  - [ ] Create structured message formatting for monitoring events
  - **Requirements:**
    - [ ] Requirement 5: ntfy Notification Integration
  - **Tests:**
    - [ ] Test 1: Verify notifications are sent without blocking main thread
    - [ ] Test 2: Verify ntfy unavailability doesn't affect core processing
    - [ ] Test 3: Verify notification messages have correct format

- [ ] **Create ntfy models and configuration**
  - [ ] Implement `NtfyMessage` model for API responses
  - [ ] Create `NtfyPollingResponse` model for batch message retrieval
  - [ ] Add `MessageProcessingContext` model for deduplication tracking
  - [ ] Update `NtfySettings` with polling configuration (interval, topics)
  - **Requirements:**
    - [ ] Requirement 8: Configuration Management
  - **Tests:**
    - [ ] Test 1: Verify ntfy models serialize/deserialize correctly
    - [ ] Test 2: Verify polling configuration is loaded properly
    - [ ] Test 3: Verify message context tracking works correctly

---

## Task 4: Phase 1 - Hello World Console Application
- [ ] **Implement command-line argument parsing**
  - [ ] Create `HelloWorldService` class for main logic
  - [ ] Add command-line argument validation (phone number required)
  - [ ] Implement help text and usage instructions
  - [ ] Add support for configuration overrides via command line
  - **Requirements:**
    - [ ] Requirement 1: Phase 1 - Hello World Message Sender
  - **Tests:**
    - [ ] Test 1: Verify application shows help when no arguments provided
    - [ ] Test 2: Verify phone number validation rejects invalid inputs
    - [ ] Test 3: Verify command-line configuration overrides work

- [ ] **Implement hello world message sending logic**
  - [ ] Integrate with `IWahaService` for message sending
  - [ ] Add "hello world" message formatting
  - [ ] Implement success/failure logging
  - [ ] Add proper exit codes (0 for success, non-zero for errors)
  - [ ] Include ntfy notification for successful sends
  - **Requirements:**
    - [ ] Requirement 1: Phase 1 - Hello World Message Sender
    - [ ] Requirement 5: ntfy Notification Integration
  - **Tests:**
    - [ ] Test 1: Verify "hello world" message is sent to correct number
    - [ ] Test 2: Verify application exits with code 0 on success
    - [ ] Test 3: Verify application exits with non-zero code on failure

- [ ] **Add configuration and error handling**
  - [ ] Set up `appsettings.json` with WAHA and ntfy configuration
  - [ ] Add environment variable support for sensitive settings
  - [ ] Implement comprehensive error handling with user-friendly messages
  - [ ] Add validation for WAHA session availability
  - **Requirements:**
    - [ ] Requirement 8: Configuration Management
    - [ ] Requirement 7: Error Handling and Resilience
  - **Tests:**
    - [ ] Test 1: Verify configuration loads from appsettings.json
    - [ ] Test 2: Verify environment variables override file settings
    - [ ] Test 3: Verify clear error messages for common issues

---

## Task 5: Phase 2 - Message Receiving Infrastructure
- [ ] **Implement ntfy polling service**
  - [ ] Create `NtfyPollingService` for HTTP GET polling
  - [ ] Add polling configuration (interval, topic name, max messages)
  - [ ] Implement message deduplication using processed message IDs
  - [ ] Add error handling for ntfy service unavailability
  - [ ] Parse ntfy JSON response into message objects
  - **Requirements:**
    - [ ] Requirement 3: Phase 2 - Message Receiver (revised for polling)
  - **Tests:**
    - [ ] Test 1: Verify polling retrieves messages from ntfy topic
    - [ ] Test 2: Verify message deduplication prevents reprocessing
    - [ ] Test 3: Verify graceful handling when ntfy is unavailable

- [ ] **Create ntfy message models and WAHA payload extraction**
  - [ ] Implement `NtfyMessage` model for ntfy API response
  - [ ] Create `NtfyPollingResponse` model for batch message retrieval
  - [ ] Add parsing logic to extract WAHA webhook payload from ntfy message
  - [ ] Implement validation for extracted webhook payloads
  - **Requirements:**
    - [ ] Requirement 3: Phase 2 - Message Receiver (revised)
  - **Tests:**
    - [ ] Test 1: Verify ntfy messages deserialize correctly
    - [ ] Test 2: Verify WAHA webhook payload extraction works
    - [ ] Test 3: Verify validation handles malformed messages gracefully

- [ ] **Set up background polling service**
  - [ ] Create `MessagePollingBackgroundService` using HostedService
  - [ ] Implement configurable polling loop with cancellation support
  - [ ] Add concurrent message processing within polling batches
  - [ ] Implement exponential backoff for polling errors
  - **Requirements:**
    - [ ] Requirement 3: Phase 2 - Message Receiver (revised)
  - **Tests:**
    - [ ] Test 1: Verify background service starts and stops correctly
    - [ ] Test 2: Verify polling loop respects configured interval
    - [ ] Test 3: Verify error handling doesn't stop polling service

---

## Task 6: Echo Response Message Processing
- [ ] **Implement echo message processor**
  - [ ] Create `EchoMessageProcessor` implementing `IMessageProcessor`
  - [ ] Add contact name extraction from webhook payload
  - [ ] Implement echo message formatting: "[contact] said [message]"
  - [ ] Add fallback to phone number when contact name unavailable
  - [ ] Handle empty messages with appropriate response
  - **Requirements:**
    - [ ] Requirement 4: Echo Response Generator
  - **Tests:**
    - [ ] Test 1: Verify echo format is correct with contact name
    - [ ] Test 2: Verify fallback to phone number when name unavailable
    - [ ] Test 3: Verify empty message handling works correctly

- [ ] **Integrate echo processor with WAHA service**
  - [ ] Add response message sending via `IWahaService`
  - [ ] Implement error handling for send failures
  - [ ] Add retry logic for message sending
  - [ ] Include success/failure logging
  - **Requirements:**
    - [ ] Requirement 4: Echo Response Generator
    - [ ] Requirement 2: WAHA Integration Service
  - **Tests:**
    - [ ] Test 1: Verify echo responses are sent back to original sender
    - [ ] Test 2: Verify retry logic works for failed sends
    - [ ] Test 3: Verify proper error handling when WAHA unavailable

- [ ] **Add ntfy notifications for message processing**
  - [ ] Send notification when message is received
  - [ ] Send notification when echo response is sent successfully
  - [ ] Send error notification when processing fails
  - [ ] Include message summary and processing time in notifications
  - **Requirements:**
    - [ ] Requirement 5: ntfy Notification Integration
  - **Tests:**
    - [ ] Test 1: Verify notifications are sent for message processing events
    - [ ] Test 2: Verify error notifications include relevant details
    - [ ] Test 3: Verify notifications don't block message processing

---

## Task 7: Configuration and Environment Setup
- [ ] **Create comprehensive configuration system**
  - [ ] Implement environment-specific `appsettings.json` files
  - [ ] Add configuration validation with data annotations
  - [ ] Support environment variable overrides for all settings
  - [ ] Add configuration documentation and examples
  - **Requirements:**
    - [ ] Requirement 8: Configuration Management
  - **Tests:**
    - [ ] Test 1: Verify configuration validation catches invalid settings
    - [ ] Test 2: Verify environment-specific overrides work
    - [ ] Test 3: Verify all required settings have appropriate defaults

- [ ] **Set up Docker configuration for development**
  - [ ] Create `docker-compose.yml` for WAHA service
  - [ ] Configure WAHA webhook URL to point to ntfy topic
  - [ ] Add environment configuration for local development
  - [ ] Create setup documentation for WAHA QR code scanning and webhook setup
  - [ ] Add health checks and service dependencies
  - **Requirements:**
    - [ ] Requirement 2: WAHA Integration Service
  - **Tests:**
    - [ ] Test 1: Verify Docker compose starts WAHA successfully
    - [ ] Test 2: Verify WAHA webhook is configured to send to ntfy topic
    - [ ] Test 3: Verify framework can connect to dockerized WAHA
    - [ ] Test 4: Verify webhook messages appear in ntfy topic

- [ ] **Create deployment configurations**
  - [ ] Add production-ready configuration templates
  - [ ] Create Dockerfile for message receiving service
  - [ ] Add Kubernetes deployment manifests (optional)
  - [ ] Document deployment procedures and requirements
  - **Requirements:**
    - [ ] Requirement 8: Configuration Management
  - **Tests:**
    - [ ] Test 1: Verify Docker image builds successfully
    - [ ] Test 2: Verify production configuration is secure
    - [ ] Test 3: Verify deployment documentation is complete

---

## Task 8: Testing Infrastructure and Coverage
- [ ] **Set up unit testing framework**
  - [ ] Configure xUnit with test projects
  - [ ] Add Moq for mocking external dependencies
  - [ ] Set up test configuration and helpers
  - [ ] Configure code coverage reporting
  - **Requirements:**
    - [ ] All requirements (testing ensures quality)
  - **Tests:**
    - [ ] Test 1: Verify all test projects run successfully
    - [ ] Test 2: Verify mocking framework works correctly
    - [ ] Test 3: Verify code coverage reports are generated

- [ ] **Create integration tests**
  - [ ] Set up test WAHA instance for integration testing
  - [ ] Create end-to-end tests for hello world functionality
  - [ ] Add ntfy polling integration tests
  - [ ] Test ntfy integration with real and mock services
  - **Requirements:**
    - [ ] All requirements (integration validation)
  - **Tests:**
    - [ ] Test 1: Verify end-to-end hello world flow works
    - [ ] Test 2: Verify ntfy polling and message processing flow works
    - [ ] Test 3: Verify error scenarios are handled correctly

- [ ] **Add performance and load testing**
  - [ ] Create ntfy polling throughput tests
  - [ ] Add memory usage and leak detection for long-running polling
  - [ ] Test concurrent message processing performance
  - [ ] Benchmark polling intervals and response times
  - **Requirements:**
    - [ ] Requirement 3: Phase 2 - Message Receiver (revised for polling)
    - [ ] Requirement 7: Error Handling and Resilience
  - **Tests:**
    - [ ] Test 1: Verify polling can handle target message throughput
    - [ ] Test 2: Verify no memory leaks during extended polling operation
    - [ ] Test 3: Verify polling intervals and response times meet targets

---

## Task 9: Documentation and Extensibility Framework
- [ ] **Create comprehensive documentation**
  - [ ] Write setup and installation guide
  - [ ] Document configuration options and examples
  - [ ] Create troubleshooting guide for common issues
  - [ ] Add API documentation for future extensibility
  - **Requirements:**
    - [ ] Requirement 9: Extensibility for AI Medical Assistant
  - **Tests:**
    - [ ] Test 1: Verify setup guide works for new developers
    - [ ] Test 2: Verify configuration examples are accurate
    - [ ] Test 3: Verify troubleshooting guide covers common scenarios

- [ ] **Design extensibility interfaces for AI integration**
  - [ ] Create pluggable message processor interface
  - [ ] Add support for multiple response generators
  - [ ] Design conversation context storage interface
  - [ ] Plan session management for multi-user scenarios
  - **Requirements:**
    - [ ] Requirement 9: Extensibility for AI Medical Assistant
  - **Tests:**
    - [ ] Test 1: Verify new message processors can be added easily
    - [ ] Test 2: Verify response generator interface supports AI integration
    - [ ] Test 3: Verify architecture supports future conversation storage

- [ ] **Create sample extensions and examples**
  - [ ] Build sample AI message processor (placeholder)
  - [ ] Create example configuration for production deployment
  - [ ] Add monitoring and observability examples
  - [ ] Document scaling and performance considerations
  - **Requirements:**
    - [ ] Requirement 9: Extensibility for AI Medical Assistant
  - **Tests:**
    - [ ] Test 1: Verify sample AI processor integrates correctly
    - [ ] Test 2: Verify production examples are realistic
    - [ ] Test 3: Verify monitoring examples provide useful insights

---

## Implementation Priority Order

### Phase 1 Priority (Minimum Viable Product)
1. **Task 1**: Project Setup and Core Infrastructure
2. **Task 2**: WAHA Integration Service Implementation  
3. **Task 3**: ntfy Notification Service Implementation
4. **Task 4**: Phase 1 - Hello World Console Application
5. **Task 7**: Configuration and Environment Setup (basic)

### Phase 2 Priority (Echo Responder via Message Receiving)
6. **Task 5**: Phase 2 - Message Receiving Infrastructure
7. **Task 6**: Echo Response Message Processing
8. **Task 8**: Testing Infrastructure and Coverage

### Phase 3 Priority (Production Ready)
9. **Task 7**: Configuration and Environment Setup (complete)
10. **Task 9**: Documentation and Extensibility Framework

## Dependencies
- **Task 2** and **Task 3** depend on **Task 1** (infrastructure)
- **Task 4** depends on **Task 2** and **Task 3** (services)
- **Task 5** and **Task 6** depend on **Task 2** (WAHA service)
- **Task 8** can run in parallel with development tasks
- **Task 9** depends on completion of core functionality

## Success Criteria
- ✅ Phase 1: Successfully send "hello world" message via console command
- ✅ Phase 2: Successfully receive WhatsApp messages via ntfy polling and send echo responses
- ✅ WAHA webhook configured to send messages to ntfy topic
- ✅ ntfy polling service retrieves and processes messages with deduplication
- ✅ ntfy notifications working for all major monitoring events
- ✅ Comprehensive test coverage (>80%) including polling scenarios
- ✅ Complete documentation for setup and usage (including WAHA webhook configuration)
- ✅ Architecture ready for AI medical assistant extension
