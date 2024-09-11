namespace Mingxiaoyu.EFCore.Audit.Tests
{
    public class EntityParserTests
    {
        [Fact]
        public void ParseEntity_ShouldParseAllPropertiesCorrectly()
        {
            // Arrange
            var input = "PrimaryKeyOne: 7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58, PrimaryKeyTwo: 3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B, SomeProperty: Test, decimalType: 123.45, IntType: 42, BoolType: true, DateTimeType: 2024-09-06T12:00:00, DoubleType: 3.14159";

            var expected = new MorePrimaryKey
            {
                PrimaryKeyOne = Guid.Parse("7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58"),
                PrimaryKeyTwo = Guid.Parse("3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B"),
                SomeProperty = "Test",
                decimalType = 123.45M,
                IntType = 42,
                BoolType = true,
                DateTimeType = new DateTime(2024, 9, 6, 12, 0, 0),
                DoubleType = 3.14159
            };

            // Act
            var result = EntityParser.ParseEntity<MorePrimaryKey>(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.PrimaryKeyOne, result.PrimaryKeyOne);
            Assert.Equal(expected.PrimaryKeyTwo, result.PrimaryKeyTwo);
            Assert.Equal(expected.SomeProperty, result.SomeProperty);
            Assert.Equal(expected.decimalType, result.decimalType);
            Assert.Equal(expected.IntType, result.IntType);
            Assert.Equal(expected.BoolType, result.BoolType);
            Assert.Equal(expected.DateTimeType, result.DateTimeType);
            Assert.Equal(expected.DoubleType, result.DoubleType);
        }

        [Fact]
        public void ParseEntityList_ShouldReturnListOfEntities()
        {
            var auditLogs = new List<AuditLog>
            {
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TableName = "MorePrimaryKey",
                    RecordID = "1",
                    Operation = "Insert",
                    OldValue = null,
                    NewValue = "PrimaryKeyOne: 7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58, PrimaryKeyTwo: 3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B, SomeProperty: Test1, decimalType: 123.45, IntType: 42, BoolType: true, DateTimeType: 2024-09-06T12:00:00, DoubleType: 3.14159",
                    ChangedBy = "User1",
                    ChangedAt = DateTime.UtcNow
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TableName = "MorePrimaryKey",
                    RecordID = "2",
                    Operation = "Update",
                    OldValue = "PrimaryKeyOne: 7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58, PrimaryKeyTwo: 3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B, SomeProperty: Test1, decimalType: 123.45, IntType: 42, BoolType: true, DateTimeType: 2024-09-06T12:00:00, DoubleType: 3.14159",
                    NewValue = "PrimaryKeyOne: 7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58, PrimaryKeyTwo: 3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B, SomeProperty: Test3, decimalType: 789.01, IntType: 99, BoolType: false, DateTimeType: 2024-11-01T10:45:00, DoubleType: 1.61803",
                    ChangedBy = "User2",
                    ChangedAt = DateTime.UtcNow
                }
            };

            var expectedList = new List<MorePrimaryKey>
            {
                new MorePrimaryKey
                {
                    PrimaryKeyOne = Guid.Parse("7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58"),
                    PrimaryKeyTwo = Guid.Parse("3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B"),
                    SomeProperty = "Test1",
                    decimalType = 123.45M,
                    IntType = 42,
                    BoolType = true,
                    DateTimeType = new DateTime(2024, 9, 6, 12, 0, 0),
                    DoubleType = 3.14159
                },
                new MorePrimaryKey
                {
                    PrimaryKeyOne = Guid.Parse("7A2E1B0F-4D9B-4E35-8C3F-2F5E5F6C1D58"),
                    PrimaryKeyTwo = Guid.Parse("3B6E3C4F-5E8D-4E25-8D3F-2A6E7F9D1A0B"),
                    SomeProperty = "Test3",
                    decimalType = 789.01M,
                    IntType = 99,
                    BoolType = false,
                    DateTimeType = new DateTime(2024, 11, 1, 10, 45, 0),
                    DoubleType =  1.61803
                }
            };

            // Act
            var result = EntityParser.ParseEntityList<MorePrimaryKey>(auditLogs);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedList.Count, result.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                Assert.Equal(expectedList[i].PrimaryKeyOne, result[i].PrimaryKeyOne);
                Assert.Equal(expectedList[i].PrimaryKeyTwo, result[i].PrimaryKeyTwo);
                Assert.Equal(expectedList[i].SomeProperty, result[i].SomeProperty);
                Assert.Equal(expectedList[i].decimalType, result[i].decimalType);
                Assert.Equal(expectedList[i].IntType, result[i].IntType);
                Assert.Equal(expectedList[i].BoolType, result[i].BoolType);
                Assert.Equal(expectedList[i].DateTimeType, result[i].DateTimeType);
                Assert.Equal(expectedList[i].DoubleType, result[i].DoubleType);
            }
        }
    }
}
