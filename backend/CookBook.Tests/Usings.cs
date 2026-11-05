// Общие using'и тестового проекта: атрибуты и Assert приходят из xUnit.
global using Xunit;

// Тесты работают с одной реальной БД и пересоздают схему перед классом,
// поэтому классы нельзя выполнять параллельно.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
