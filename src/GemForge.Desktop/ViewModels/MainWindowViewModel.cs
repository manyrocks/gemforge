using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GemForge.Domain.Models;
using GemForge.Infrastructure.FileFormats;
using GemForge.Infrastructure.Rendering;

namespace GemForge.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private GemDesign? _currentDesign;

    [ObservableProperty]
    private Mesh? _currentMesh;

    [ObservableProperty]
    private string _statusText = "Ready. Open a .asc gem file to begin.";

    [RelayCommand]
    private async Task OpenFile()
    {
        try
        {
            // Get the main window's storage provider
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null)
                return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Gem Design",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("GemCad Files")
                    {
                        Patterns = new[] { "*.asc" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                var path = file.Path.LocalPath;

                StatusText = $"Loading {file.Name}...";

                // Parse the .asc file
                CurrentDesign = AscParser.ParseFile(path);

                if (CurrentDesign != null)
                {
                    StatusText = $"Generating mesh for {CurrentDesign.Name}...";

                    // Debug output
                    System.Diagnostics.Debug.WriteLine($"Design: {CurrentDesign.Name}");
                    System.Diagnostics.Debug.WriteLine($"Facets: {CurrentDesign.Facets.Count}");
                    System.Diagnostics.Debug.WriteLine($"Index Gear: {CurrentDesign.IndexGear}");

                    // Generate 3D mesh
                    CurrentMesh = MeshGenerator.GenerateMesh(CurrentDesign);

                    // Debug output
                    System.Diagnostics.Debug.WriteLine($"Mesh generated:");
                    System.Diagnostics.Debug.WriteLine($"  Vertices: {CurrentMesh.VertexCount}");
                    System.Diagnostics.Debug.WriteLine($"  Triangles: {CurrentMesh.TriangleCount}");
                    System.Diagnostics.Debug.WriteLine($"  Edges: {CurrentMesh.EdgeCount}");

                    if (CurrentMesh.VertexCount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"First vertex: {CurrentMesh.Vertices[0]}");
                        var bounds = CurrentMesh.GetBounds();
                        System.Diagnostics.Debug.WriteLine($"Bounds: {bounds.min} to {bounds.max}");
                    }

                    StatusText = $"Loaded: {CurrentDesign.Name} - " +
                                $"{CurrentMesh.VertexCount} vertices, " +
                                $"{CurrentMesh.TriangleCount} triangles, " +
                                $"{CurrentMesh.EdgeCount} edges";
                }
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    [RelayCommand]
    private void About()
    {
        StatusText = "GemForge v0.1 - 3D Gem Design Viewer";
    }
}
