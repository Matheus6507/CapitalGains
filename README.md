# Capital Gains Tax Calculator

A command-line interface (CLI) application that calculates capital gains tax on stock trading operations according to Brazilian tax regulations.

## Technical and Architectural Decisions

### Architecture Overview

The solution follows a clean architecture pattern with clear separation of concerns:

```
CapitalGains/
 Domain/              # Core business logic and entities
    Position.cs      # Position tracking with weighted average price
    Operation.cs     # Operation model with JSON serialization
    TaxResult.cs     # Tax calculation result model
 Application/         # Application services
    CapitalGainsService.cs  # Main business logic orchestration
 Program.cs          # CLI interface and JSON I/O handling
```

### Key Design Principles

1. **Single Responsibility**: Each class has a well-defined, single responsibility
2. **Immutable State**: Position state changes only through controlled methods
3. **Stateless Service**: Each operation list is processed independently
4. **Pure Functions**: Tax calculations are deterministic and side-effect free

### Domain Model

#### Position Class
- **Weighted Average Price (WAP)**: Automatically calculated and rounded to 2 decimal places
- **Loss Accumulation**: Tracks losses to offset future profits
- **Tax Calculation**: Implements all business rules including exemptions

#### Operation Class
- **JSON Serialization**: Proper mapping for kebab-case JSON properties (`operation`, `unit-cost`, `quantity`)
- **Type Safety**: Boolean properties for operation type checking (`IsBuy`, `IsSell`)
- **Value Calculation**: Computed property for total operation value

#### TaxResult Class
- **JSON Output**: Properly formatted tax result with `tax` property
- **Decimal Precision**: Maintains accurate monetary calculations

### Business Rules Implementation

1. **Tax Rate**: Fixed 20% on taxable profits
2. **Exemption Threshold**: Operations ? R$ 20,000 are tax-exempt
3. **Weighted Average Price**: Recalculated on each purchase using the formula:
   ```
   new_avg = ((current_qty * current_avg) + (bought_qty * bought_price)) / (current_qty + bought_qty)
   ```
4. **Loss Carryforward**: Accumulated losses offset future profits
5. **Exempt Operation Handling**: Exempt operations don't consume accumulated losses but still accumulate losses if they result in a loss
6. **Rounding**: All monetary calculations use `MidpointRounding.AwayFromZero` to 2 decimal places

## Framework and Library Justification

### Core Framework
- **.NET 9**: Latest LTS version providing performance improvements and modern C# features
- **System.Text.Json**: Built-in JSON serialization with kebab-case naming policy support

### Testing Framework
- **xUnit**: Industry-standard testing framework for .NET
- **No additional mocking libraries**: The domain is pure and doesn't require external dependencies

### Design Rationale
- **Minimal Dependencies**: Avoids unnecessary complexity and maintains simplicity
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Containerization Ready**: Includes Docker support for consistent deployment

## Compilation and Execution Instructions

### Prerequisites
- .NET 9 SDK installed
- Compatible with Windows, Linux, and macOS

### Build Instructions

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Build in release mode
dotnet build -c Release
```

### Running the Application

#### Method 1: Direct Execution
```bash
cd CapitalGains
dotnet run
```

#### Method 2: From Solution Root
```bash
dotnet run --project CapitalGains
```

#### Method 3: Published Executable
```bash
# Publish the application
dotnet publish CapitalGains -c Release -o ./publish

# Run the published executable
./publish/CapitalGains        # Linux/macOS
.\publish\CapitalGains.exe    # Windows
```

#### Method 4: Docker
```bash
# Build and run with Docker
docker build -t capital-gains .
docker run -i capital-gains
```

### Input/Output Format

The application reads from stdin and writes to stdout:

**Input Format:**
```json
[{"operation":"buy", "unit-cost":10.00, "quantity": 100}, {"operation":"sell", "unit-cost":15.00, "quantity": 50}]
[{"operation":"buy", "unit-cost":20.00, "quantity": 1000}, {"operation":"sell", "unit-cost":25.00, "quantity": 500}]

```
(Empty line terminates input)

**Output Format:**
```json
[{"tax":0},{"tax":0}]
[{"tax":0},{"tax":500}]
```

### Usage Examples

#### From File
```bash
echo '[{"operation":"buy", "unit-cost":10.00, "quantity": 100}]' | dotnet run --project CapitalGains
```

#### Interactive Mode
```bash
dotnet run --project CapitalGains
# Type JSON operations line by line
# Press Enter twice to finish
```

#### Using Input Redirection
```bash
dotnet run --project CapitalGains < input.txt
```

## Test Execution Instructions

### Running Tests

```bash
# Run all tests
dotnet test

# With detailed output
dotnet test -v normal

# With coverage (if coverage tools are installed)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

```
CapitalGains.Tests/
 Unit/                    # Unit tests for business logic
     CapitalGainsServiceTests.cs  # Service layer tests (10 tests)
```