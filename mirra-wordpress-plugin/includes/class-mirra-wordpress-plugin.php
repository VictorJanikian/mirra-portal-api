<?php

/**
 * The core plugin class.
 * 
 * A class definition that includes attributes and functions used across both the
 * public-facing side of the site and the admin area.
 *
 * This is used to define internationalization, admin-specific hooks, and
 * public-facing site hooks.
 *
 * Also maintains the unique identifier of this plugin as well as the current
 * version of the plugin.
 *
 * @since      1.0.0
 * @package    Mirra_Wordpress_Plugin
 * @subpackage Mirra_Wordpress_Plugin/includes
 */
class Mirra_Wordpress_Plugin {

	/**
	 * The loader that's responsible for maintaining and registering all hooks that power
	 * the plugin.
	 *

	 * @access   protected
	 * @var      Mirra_Wordpress_Plugin_Loader    $loader    Maintains and registers all hooks for the plugin.
	 */
	protected $loader;

	/**
	 * The unique identifier of this plugin.
	 *

	 * @access   protected
	 * @var      string    $plugin_name    The string used to uniquely identify this plugin.
	 */
	protected $plugin_name;

	/**
	 * The current version of the plugin.
	 *

	 * @access   protected
	 * @var      string    $version    The current version of the plugin.
	 */
	protected $version;

	/**
	 * Define the core functionality of the plugin.
	 *
	 * Set the plugin name and the plugin version that can be used throughout the plugin.
	 * Load the dependencies, define the locale, and set the hooks for the admin area and
	 * the public-facing side of the site.
	 *

	 */
	public function __construct() {
		if ( defined( 'MIRRA_WORDPRESS_PLUGIN_VERSION' ) ) {
			$this->version = MIRRA_WORDPRESS_PLUGIN_VERSION;
		} else {
			$this->version = '1.0.0';
		}
		$this->plugin_name = 'mirra-wordpress-plugin';

		$this->load_dependencies();
		$this->set_locale();
		$this->define_admin_hooks();
		$this->define_public_hooks();

	}

	/**
	 * Load the required dependencies for this plugin.
	 *
	 * Include the following files that make up the plugin:
	 *
	 * - Mirra_Wordpress_Plugin_Loader. Orchestrates the hooks of the plugin.
	 * - Mirra_Wordpress_Plugin_i18n. Defines internationalization functionality.
	 * - Mirra_Wordpress_Plugin_Admin. Defines all hooks for the admin area.
	 * - Mirra_Wordpress_Plugin_Public. Defines all hooks for the public side of the site.
	 *
	 * Create an instance of the loader which will be used to register the hooks
	 * with WordPress.
	 *

	 * @access   private
	 */
	private function load_dependencies() {

		/**
		 * The class responsible for orchestrating the actions and filters of the
		 * core plugin.
		 */
  		require_once MIRRA_PLUGIN_DIR . 'includes/class-mirra-wordpress-plugin-loader.php';

		/**
		 * The class responsible for defining internationalization functionality
		 * of the plugin.
		 */
  		require_once MIRRA_PLUGIN_DIR . 'includes/class-mirra-wordpress-plugin-i18n.php';

		/**
		 * The class responsible for defining all actions that occur in the admin area.
		 */

		
  		require_once MIRRA_PLUGIN_DIR . 'admin/class-mirra-wordpress-plugin-admin.php';

		/**
		 * The class responsible for defining all actions that occur in the public-facing
		 * side of the site.
		 */
  		require_once MIRRA_PLUGIN_DIR . 'public/class-mirra-wordpress-plugin-public.php';

		$this->loader = new Mirra_Wordpress_Plugin_Loader();

	}

	/**
	 * Define the locale for this plugin for internationalization.
	 *
	 * Uses the Mirra_Wordpress_Plugin_i18n class in order to set the domain and to register the hook
	 * with WordPress.
	 *

	 * @access   private
	 */
	private function set_locale() {

		$plugin_i18n = new Mirra_Wordpress_Plugin_i18n();

		$this->loader->add_action( 'plugins_loaded', $plugin_i18n, 'load_plugin_textdomain' );

	}

	/**
	 * Register all of the hooks related to the admin area functionality
	 * of the plugin.
	 *

	 * @access   private
	 */
	private function define_admin_hooks() {

		$plugin_admin = new Mirra_Wordpress_Plugin_Admin( $this->get_mirra_wordpress_plugin(), $this->get_version() );

		$this->loader->add_action( 'admin_enqueue_scripts', $plugin_admin, 'enqueue_styles' );
		$this->loader->add_action( 'admin_enqueue_scripts', $plugin_admin, 'enqueue_scripts' );

	}

	/**
	 * Register all of the hooks related to the public-facing functionality
	 * of the plugin.
	 *

	 * @access   private
	 */
	private function define_public_hooks() {

		$plugin_public = new Mirra_Wordpress_Plugin_Public( $this->get_mirra_wordpress_plugin(), $this->get_version() );

		$this->loader->add_action( 'wp_enqueue_scripts', $plugin_public, 'enqueue_styles' );
		$this->loader->add_action( 'wp_enqueue_scripts', $plugin_public, 'enqueue_scripts' );

	}

	/**
	 * Run the loader to execute all of the hooks with WordPress.
	 */
	public function run() {
		$this->loader->run();
	}

	/**
	 * The name of the plugin used to uniquely identify it within the context of
	 * WordPress and to define internationalization functionality.
	 *
	 * @return    string    The name of the plugin.
	 */
	public function get_mirra_wordpress_plugin() {
		return $this->plugin_name;
	}

	/**
	 * The reference to the class that orchestrates the hooks with the plugin.
	 *
	 * @return    Mirra_wordpress_Plugin_Loader    Orchestrates the hooks of the plugin.
	 */
	public function get_loader() {
		return $this->loader;
	}

	/**
	 * Retrieve the version number of the plugin.
	 *
	 * @return    string    The version number of the plugin.
	 */
	public function get_version() {
		return $this->version;
	}

}
