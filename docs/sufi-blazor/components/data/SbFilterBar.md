# SbFilterBar

A horizontal bar component for displaying filter controls with search and clear functionality.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| SearchValue | string? | null | Search input value (two-way bindable) |
| SearchPlaceholder | string | "Search..." | Search input placeholder |
| ShowSearch | bool | true | Whether to show search input |
| ShowClearAll | bool | true | Whether to show clear all button |
| ActiveFilterCount | int | 0 | Number of active filters |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| SearchValueChanged | EventCallback\<string?\> | Fired when search value changes |
| OnClearAll | EventCallback | Fired when clear all is clicked |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Filter controls |
| ActionsTemplate | RenderFragment | Additional action buttons |

### Template Usage Examples

```razor
<SbFilterBar @bind-SearchValue="searchText" OnClearAll="ClearFilters">
    <SbSimpleSelect @bind-Value="statusFilter" Placeholder="Status">
        <SbSelectOption Value="">All Status</SbSelectOption>
        <SbSelectOption Value="active">Active</SbSelectOption>
        <SbSelectOption Value="inactive">Inactive</SbSelectOption>
    </SbSimpleSelect>
    
    <SbDatePicker @bind-Value="dateFilter" Placeholder="Date" />
    
    <ActionsTemplate>
        <SbButton Variant="SbButtonVariant.Outline" OnClick="ExportData">
            <SbIcon Name="download" /> Export
        </SbButton>
    </ActionsTemplate>
</SbFilterBar>
```

## CSS Classes

- `sb-filter-bar` - Base class
- `sb-filter-bar__search` - Search input container
- `sb-filter-bar__filters` - Filters container
- `sb-filter-bar__actions` - Actions container
- `sb-filter-bar__clear` - Clear all button
- `sb-filter-bar__count` - Active filter count badge

## Examples

### Basic Usage

```razor
<SbFilterBar @bind-SearchValue="search">
    <SbSimpleSelect @bind-Value="category">
        <SbSelectOption Value="">All Categories</SbSelectOption>
        @foreach (var cat in categories)
        {
            <SbSelectOption Value="@cat.Id.ToString()">@cat.Name</SbSelectOption>
        }
    </SbSimpleSelect>
</SbFilterBar>
```

### With Multiple Filters

```razor
<SbFilterBar @bind-SearchValue="searchQuery" 
             ActiveFilterCount="@activeFilterCount"
             OnClearAll="ResetFilters">
    <SbSimpleSelect @bind-Value="statusFilter" Placeholder="Status">
        <SbSelectOption Value="">All</SbSelectOption>
        <SbSelectOption Value="pending">Pending</SbSelectOption>
        <SbSelectOption Value="approved">Approved</SbSelectOption>
        <SbSelectOption Value="rejected">Rejected</SbSelectOption>
    </SbSimpleSelect>
    
    <SbDateRangePicker @bind-Value="dateRange" Placeholder="Date Range" />
    
    <SbMultiSelect TItem="string" TValue="string"
                   Items="@tags"
                   @bind-Values="selectedTags"
                   Placeholder="Tags" />
</SbFilterBar>
```

### Search Only

```razor
<SbFilterBar @bind-SearchValue="query" 
             ShowClearAll="false">
</SbFilterBar>
```

### With DataGrid

```razor
<SbFilterBar @bind-SearchValue="search" OnClearAll="ClearAllFilters">
    <SbSimpleSelect @bind-Value="departmentFilter">
        <SbSelectOption Value="">All Departments</SbSelectOption>
        @foreach (var dept in departments)
        {
            <SbSelectOption Value="@dept">@dept</SbSelectOption>
        }
    </SbSimpleSelect>
</SbFilterBar>

<SbDataGrid TItem="Employee" Items="@FilteredEmployees">
    <!-- Columns -->
</SbDataGrid>
```
