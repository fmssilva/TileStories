import 'package:flutter/material.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import '../../utils/i18n/models/translatable_string.dart';

// Freezed will generate immutable models with copyWith and equality
part 'nav_item.freezed.dart';

/// Navigation Item Model

@freezed
sealed class NavItem with _$NavItem {
  /// Creates a navigation item
  ///
  /// [id] - Unique identifier (used for routing and lookups)
  /// [path] - URL path (e.g., '/', '/explore', '/about')
  /// [label] - Display name with translations (PT/EN)
  /// [builder] - Function that builds the page widget
  /// [labelShort] - Optional short label for limited space (mobile)
  /// [parent] - Parent item for breadcrumb hierarchy
  /// [children] - Child items for nested navigation
  /// [metadata] - Configuration (visibility, auth, ordering, LAYOUT)
  const factory NavItem({
    required String id,
    required String path,
    required TranslatableString label,
    required WidgetBuilder builder,
    TranslatableString? labelShort,
    NavItem? parent,
    @Default([]) List<NavItem> children,
    NavMetadata? metadata,
  }) = _NavItem;
}

/// Navigation metadata for filtering and configuration of this Navigation Item
///
/// FIELDS:
/// - [showInNav] - Show in main navigation menu? (default: true)
/// - [showInBreadcrumb] - Show in breadcrumb trail? (default: true)
/// - [requiresAuth] - Requires user authentication? (default: false)
/// - [order] - Sort order in navigation (lower numbers first)
/// - [layoutConfig] - Layout configuration (NEW IN V2!)
@freezed
sealed class NavMetadata with _$NavMetadata {
  const factory NavMetadata({
    /// Show this item in navigation menus (header, footer, mobile menu)
    @Default(true) bool showInNav,

    /// Show this item in breadcrumb trail
    /// Set to false for utility pages (login, 404, etc.)
    @Default(true) bool showInBreadcrumb,

    /// Sort order in navigation menus (ascending)
    /// Lower numbers appear first (e.g., Home = 1, Explore = 2)
    @Default(0) int order,

    /// Require authentication to access this route
    /// If true and user not authenticated → redirect to login
    @Default(false) bool requiresAuth,
  }) = _NavMetadata;
}
