﻿using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class EntityFixture
    {
        [Fact]
        public void Entity_Should_Not_Be_Equal_To_Null()
        {
            var e1 = TestEntity.Create(1);

            ((object)null).Should().NotBe(e1);
        }

        [Fact]
        public void Entity_Should_Not_Be_Equal_To_Object()
        {
            var e1 = TestEntity.Create(1);

            new object().Should().NotBe(e1);
        }

        [Fact]
        public void Same_Entity_Types_With_Different_IDs_Should_Not_Be_Equal()
        {
            TestEntity.Create(1).Should().NotBe(TestEntity.Create(2));
        }

        [Fact]
        public void Entity_With_No_ID_Should_Be_Transient()
        {
            new TestEntity().IsTransient().Should().BeTrue();
        }

        [Fact]
        public void Transient_Entities_Of_Same_Type_Should_Be_Not_Be_Equal()
        {
            new TestEntity().Should().NotBe(new TestEntity());
        }

        [Fact]
        public void Transient_Entities_Of_Different_Types_Should_Be_Not_Be_Equal()
        {
            new TestEntity().Should().NotBe(new OtherTestEntity());
        }

        [Fact]
        public void Transient_Entities_Should_Have_Different_HashCodes()
        {
            new TestEntity().GetHashCode().Should().NotBe(new TestEntity().GetHashCode());
        }

        [Fact]
        public void Transient_Entities_Of_Different_Types_Should_Have_Different_HashCodes()
        {
            new TestEntity().GetHashCode().Should().NotBe(new OtherTestEntity().GetHashCode());
        }

        [Fact]
        public void Persistent_Entities_Of_Same_Type_With_Same_ID_Should_Have_The_Same_HashCode()
        {
            TestEntity.Create(1).GetHashCode().Should().Be(TestEntity.Create(1).GetHashCode());
        }

        [Fact]
        public void Persistent_Entities_Of_Different_Types_With_Same_ID_Should_Have_Different_HashCodes()
        {
            TestEntity.Create(1).GetHashCode().Should().NotBe(OtherTestEntity.Create(1).GetHashCode());
        }

        private class TestEntity : EntityBase<int>
        {
            public override int ID { get; protected set; }

            public static TestEntity Create(int id)
            {
                return new TestEntity()
                {
                    ID = id
                };
            }
        }

        private class OtherTestEntity : EntityBase<int>
        {
            public override int ID { get; protected set; }

            public static OtherTestEntity Create(int id)
            {
                return new OtherTestEntity()
                {
                    ID = id
                };
            }
        }
    }
}